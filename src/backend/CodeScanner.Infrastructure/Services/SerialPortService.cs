using System.IO.Ports;
using System.Text;
using CodeScanner.Application.Services;
using Microsoft.Extensions.Logging;

namespace CodeScanner.Infrastructure.Services;

public class SerialPortService : ISerialPortService, IDisposable
{
    private readonly ILogger<SerialPortService> _logger;
    private SerialPort? _serialPort;
    private readonly StringBuilder _buffer = new();
    private readonly object _lock = new();

    public event Action<string>? DataReceived;
    public event Action<bool, string>? ConnectionStateChanged;

    public bool IsConnected => _serialPort != null && _serialPort.IsOpen;
    public string? CurrentPort { get; private set; }
    public int CurrentBaudRate { get; private set; } = 9600;

    public SerialPortService(ILogger<SerialPortService> logger)
    {
        _logger = logger;
    }

    public List<string> GetAvailablePorts()
    {
        try
        {
            var ports = SerialPort.GetPortNames().Distinct().OrderBy(p => p).ToList();
            if (!ports.Contains("COM_SIMULATOR"))
            {
                ports.Add("COM_SIMULATOR");
            }
            return ports;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting serial port names");
            return new List<string> { "COM_SIMULATOR" };
        }
    }

    public bool Connect(string portName, int baudRate)
    {
        lock (_lock)
        {
            try
            {
                Disconnect();

                CurrentPort = portName;
                CurrentBaudRate = baudRate;

                if (string.Equals(portName, "COM_SIMULATOR", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Connected to virtual COM Simulator");
                    ConnectionStateChanged?.Invoke(true, "Connected (Simulator)");
                    return true;
                }

                _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 2000,
                    WriteTimeout = 2000,
                    DtrEnable = true,
                    RtsEnable = true
                };

                _serialPort.DataReceived += OnSerialDataReceived;
                _serialPort.Open();

                _logger.LogInformation("Serial port {Port} opened successfully at {BaudRate} baud", portName, baudRate);
                ConnectionStateChanged?.Invoke(true, $"Connected to {portName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to serial port {Port}", portName);
                Disconnect();
                ConnectionStateChanged?.Invoke(false, ex.Message);
                return false;
            }
        }
    }

    public void Disconnect()
    {
        lock (_lock)
        {
            try
            {
                if (_serialPort != null)
                {
                    _serialPort.DataReceived -= OnSerialDataReceived;
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.Dispose();
                    _serialPort = null;
                }
                _buffer.Clear();
                CurrentPort = null;
                ConnectionStateChanged?.Invoke(false, "Disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting serial port");
            }
        }
    }

    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;

            string incoming = _serialPort.ReadExisting();
            lock (_lock)
            {
                _buffer.Append(incoming);
                ProcessBuffer();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from serial port");
        }
    }

    private void ProcessBuffer()
    {
        string currentText = _buffer.ToString();
        int startIndex = currentText.IndexOf('@');
        while (startIndex >= 0)
        {
            int endIndex = currentText.IndexOf('^', startIndex);
            if (endIndex > startIndex)
            {
                string completePacket = currentText.Substring(startIndex, endIndex - startIndex + 1);
                _logger.LogInformation("Extracted complete packet: {Packet}", completePacket);
                DataReceived?.Invoke(completePacket);

                currentText = currentText.Substring(endIndex + 1);
                _buffer.Clear();
                _buffer.Append(currentText);
                startIndex = currentText.IndexOf('@');
            }
            else
            {
                // Incomplete packet; trim any garbage preceding '@'
                if (startIndex > 0)
                {
                    currentText = currentText.Substring(startIndex);
                    _buffer.Clear();
                    _buffer.Append(currentText);
                }
                break;
            }
        }
    }

    public void Dispose()
    {
        Disconnect();
    }
}
