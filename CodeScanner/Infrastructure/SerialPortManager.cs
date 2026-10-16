using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utility;

namespace CodeScanner.Infrastructure
{
    /// <summary>
    /// Owns the single physical jig serial connection for the whole application.
    ///
    /// Why this exists:
    ///  * Close-then-reopen of the same COM port must be deterministic. Closing on a
    ///    background thread and immediately calling Open() on the same port name yields
    ///    "Access to the port 'COMx' is denied" because Windows has not released the handle.
    ///  * SerialPort.GetPortNames() can return names with trailing garbage on some USB-serial
    ///    drivers (e.g. "COM3\0" / "COM3c"), which made exact-match presence checks report a
    ///    disconnect while the cable was still attached.
    ///  * A newly scanned unit must be able to pre-empt an in-flight read of a previous scan
    ///    instead of queueing behind it for the full watchdog period.
    ///  * A port left open by an abandoned scan (closed tab, client-side error) must be
    ///    released automatically so the next scan can open it.
    /// </summary>
    public static class SerialPortManager
    {
        // Serializes every operation on the physical port (open/write/read/close).
        public static readonly object PortLock = new object();

        // Open retry policy for transient "access denied"/"in use" errors right after a close.
        private const int OpenMaxAttempts = 6;
        private const int OpenInitialBackoffMs = 120;

        // Upper bound for a synchronous Close(); USB drivers can stall forever after a yank.
        private const int CloseTimeoutMs = 2500;

        private const int WriteTimeoutMs = 2000;

        // Release a port nobody has touched for this long (abandoned scan / closed browser tab).
        private static readonly TimeSpan IdleReleaseAfter = TimeSpan.FromSeconds(60);

        private static SerialPort _port;
        private static string _portName;
        private static long _lastActivityTicks = DateTime.UtcNow.Ticks;

        // Incremented for every new scan. Readers compare against their captured value to
        // detect that a newer scan has taken over and they should stop immediately.
        private static long _scanGeneration;

        private static readonly Timer _idleTimer = new Timer(OnIdleTick, null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));

        private static readonly Regex PortNameRegex = new Regex(@"^COM\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>Current port instance. Only touch while holding <see cref="PortLock"/>.</summary>
        public static SerialPort Port { get { return _port; } }

        public static bool IsOpen
        {
            get
            {
                var p = _port;
                try { return p != null && p.IsOpen; }
                catch { return false; }
            }
        }

        /// <summary>Signals any in-flight read loop to abort. Call BEFORE acquiring <see cref="PortLock"/>.</summary>
        public static long BeginNewScan()
        {
            return Interlocked.Increment(ref _scanGeneration);
        }

        public static long CurrentGeneration
        {
            get { return Interlocked.Read(ref _scanGeneration); }
        }

        public static void Touch()
        {
            Interlocked.Exchange(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Normalizes a COM port name, stripping the trailing junk some drivers append.
        /// Returns the original (trimmed) value for non-COM names (e.g. SIMULATOR).
        /// </summary>
        public static string NormalizePortName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            var trimmed = name.Trim().TrimEnd('\0');
            var m = PortNameRegex.Match(trimmed);
            return m.Success ? m.Value.ToUpperInvariant() : trimmed;
        }

        /// <summary>Sanitized, de-duplicated, naturally ordered list of system COM ports.</summary>
        public static string[] GetAvailablePorts()
        {
            try
            {
                return SerialPort.GetPortNames()
                    .Select(NormalizePortName)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n.Length)
                    .ThenBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Log.Error("[SerialPortManager] GetPortNames failed: " + ex.Message);
                return new string[0];
            }
        }

        /// <summary>True when Windows still enumerates the port (tolerant to driver name quirks).</summary>
        public static bool IsPortPresent(string portName)
        {
            var target = NormalizePortName(portName);
            if (string.IsNullOrEmpty(target)) return false;
            return GetAvailablePorts().Any(p => string.Equals(p, target, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Closes any existing connection and opens a fresh one. Must be called while holding <see cref="PortLock"/>.
        /// Retries transient "access denied"/"in use" errors with exponential backoff.
        /// </summary>
        public static SerialPort Open(string portName, int baudRate, int readTimeoutMs)
        {
            var name = NormalizePortName(portName);

            CloseInternal("re-open for new scan");

            if (!IsPortPresent(name))
            {
                throw new IOException("COM Port '" + name + "' is not available on this system. Check the USB cable and select the port again.");
            }

            Exception last = null;
            var backoff = OpenInitialBackoffMs;

            for (var attempt = 1; attempt <= OpenMaxAttempts; attempt++)
            {
                var sp = new SerialPort
                {
                    PortName = name,
                    BaudRate = baudRate > 0 ? baudRate : 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = readTimeoutMs,
                    WriteTimeout = WriteTimeoutMs
                };

                try
                {
                    sp.Open();

                    // Work-around for the .NET Framework SerialStream finalizer bug: if the USB
                    // device is yanked, the finalizer can throw on the GC thread and take down
                    // the whole worker process. We re-register before an explicit close.
                    TrySuppressStreamFinalizer(sp);

                    // Drop any bytes left in the driver buffer from a previous unit/scan.
                    try { sp.DiscardInBuffer(); sp.DiscardOutBuffer(); } catch { }

                    _port = sp;
                    _portName = name;
                    Touch();

                    if (attempt > 1)
                    {
                        Log.Info("[SerialPortManager] Opened " + name + " on attempt " + attempt + ".");
                    }
                    return sp;
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    last = ex;
                    SafeDispose(sp);
                    Log.Warn("[SerialPortManager] Open " + name + " attempt " + attempt + "/" + OpenMaxAttempts + " failed: " + ex.Message);

                    if (!IsPortPresent(name))
                    {
                        break; // physically gone - retrying won't help
                    }
                    Thread.Sleep(backoff);
                    backoff = Math.Min(backoff * 2, 1000);
                }
                catch
                {
                    SafeDispose(sp);
                    throw;
                }
            }

            var reason = last is UnauthorizedAccessException
                ? "COM Port '" + name + "' is in use by another application (or still being released). Close other serial tools and scan again."
                : "Unable to open COM Port '" + name + "': " + (last != null ? last.Message : "unknown error");
            throw new IOException(reason, last);
        }

        /// <summary>Writes one line. Caller must hold <see cref="PortLock"/>.</summary>
        public static void WriteLine(string text)
        {
            var p = _port;
            if (p == null || !p.IsOpen) throw new InvalidOperationException("Serial port is not open.");
            p.WriteLine(text);
            Touch();
        }

        /// <summary>Synchronously closes the port (bounded). Caller should hold <see cref="PortLock"/>.</summary>
        public static void Close(string reason)
        {
            CloseInternal(reason);
        }

        private static void CloseInternal(string reason)
        {
            var p = _port;
            var name = _portName;
            _port = null;
            _portName = null;
            if (p == null) return;

            // Close synchronously so the OS handle is released before anyone re-opens the port,
            // but never let a stalled USB driver hang the request thread indefinitely.
            var closeTask = Task.Run(() =>
            {
                try
                {
                    TryReRegisterStreamFinalizer(p);
                    if (p.IsOpen)
                    {
                        try { p.DiscardInBuffer(); } catch { }
                        p.Close();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("[SerialPortManager] Error closing " + name + ": " + ex.Message);
                }
                finally
                {
                    SafeDispose(p);
                }
            });

            if (!closeTask.Wait(CloseTimeoutMs))
            {
                Log.Warn("[SerialPortManager] Close of " + name + " exceeded " + CloseTimeoutMs + "ms (driver stalled). Continuing in background.");
            }
            else
            {
                Log.Info("[SerialPortManager] Closed " + name + " (" + reason + ").");
                // Allow the Windows kernel USB-serial driver a small grace window (50ms) to release
                // the device handle before any immediate CreateFile/Open attempt on the same port name.
                try { Thread.Sleep(50); } catch { }
            }
        }

        private static void OnIdleTick(object state)
        {
            try
            {
                if (_port == null) return;
                var idleFor = DateTime.UtcNow - new DateTime(Interlocked.Read(ref _lastActivityTicks), DateTimeKind.Utc);
                if (idleFor < IdleReleaseAfter) return;

                // Never fight an active request for the port.
                if (!Monitor.TryEnter(PortLock)) return;
                try
                {
                    if (_port != null)
                    {
                        CloseInternal("idle for " + (int)idleFor.TotalSeconds + "s");
                    }
                }
                finally
                {
                    Monitor.Exit(PortLock);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[SerialPortManager] Idle watchdog error: " + ex.Message);
            }
        }

        private static void TrySuppressStreamFinalizer(SerialPort sp)
        {
            try { GC.SuppressFinalize(sp.BaseStream); } catch { }
        }

        private static void TryReRegisterStreamFinalizer(SerialPort sp)
        {
            try { if (sp.IsOpen) GC.ReRegisterForFinalize(sp.BaseStream); } catch { }
        }

        private static void SafeDispose(SerialPort sp)
        {
            try { sp.Dispose(); } catch { }
        }
    }
}
