#if DEBUG
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Entity;

namespace CodeScanner.Simulation
{
    /// <summary>
    /// Encapsulates all mock telemetry and hardware jig simulation logic.
    /// Kept completely isolated from production serial communication controllers.
    /// </summary>
    public static class JigSimulationEngine
    {
        public const string SimulatorPortName = "SIMULATOR";

        /// <summary>
        /// Delay in milliseconds between each parameter advancement.
        /// Set to 1500ms (1.5 seconds) per parameter step as requested.
        /// </summary>
        public static int StepDelayMs { get; set; } = 1500;

        // Tracks step-by-step test progression per barcode
        private static readonly ConcurrentDictionary<string, int> _activeSteps = new ConcurrentDictionary<string, int>();
        private static readonly ConcurrentDictionary<string, string> _lastFrames = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Checks if the provided port string is the virtual simulator port.
        /// </summary>
        public static bool IsSimulatorPort(string port)
        {
            return string.Equals(port, SimulatorPortName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Resets the simulation step counter for a barcode (or all barcodes if null).
        /// </summary>
        public static void Reset(string barcode = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                _activeSteps.Clear();
                _lastFrames.Clear();
            }
            else
            {
                int dummy;
                _activeSteps.TryRemove(barcode, out dummy);
                string dummyStr;
                _lastFrames.TryRemove(barcode, out dummyStr);
            }
        }

        /// <summary>
        /// Retrieves the current simulation step for monitoring.
        /// </summary>
        public static int GetCurrentStep(string barcode)
        {
            string key = barcode ?? "DEFAULT";
            int step;
            return _activeSteps.TryGetValue(key, out step) ? step : 0;
        }

        /// <summary>
        /// Retrieves the last generated frame for monitoring.
        /// </summary>
        public static string GetLastFrame(string barcode)
        {
            string key = barcode ?? "DEFAULT";
            string frame;
            return _lastFrames.TryGetValue(key, out frame) ? frame : string.Empty;
        }

        /// <summary>
        /// Generates the next telemetry frame in the step-by-step test sequence.
        /// Advances by 1 parameter per call until all parameters are PASS/OK.
        /// </summary>
        public static string GetNextFrame(enSetting setting, string barcode, bool isRecurrence, bool isFail = false)
        {
            string simKey = barcode ?? "DEFAULT";
            int totalParams = setting != null && setting.SettingInfo != null && setting.SettingInfo.Count > 0
                ? setting.SettingInfo.Count
                : 23;

            int step;
            if (!isRecurrence)
            {
                step = 0;
                _activeSteps[simKey] = 0;
            }
            else
            {
                step = _activeSteps.AddOrUpdate(simKey, 1, (k, v) => v + 1);
                if (step >= totalParams - 1)
                {
                    int dummy;
                    _activeSteps.TryRemove(simKey, out dummy);
                }
            }

            // Hardware read delay between sequential parameters
            if (StepDelayMs > 0)
            {
                Thread.Sleep(StepDelayMs);
            }

            string frame = BuildFrame(setting, barcode, step, isFail, totalParams);
            _lastFrames[simKey] = frame;
            return frame;
        }

        private static string BuildFrame(enSetting setting, string barcode, int step, bool isFail, int totalParams)
        {
            List<string> tokens = new List<string>
            {
                "@",
                "SPP_0.1",
                "SPP_C_LT_1.0",
                setting != null && setting.Model != null && !string.IsNullOrEmpty(setting.Model.Name)
                    ? setting.Model.Name
                    : "VIKRANT_PRO_1250VA_12V"
            };

            for (int i = 0; i < totalParams; i++)
            {
                string pName = setting != null && setting.SettingInfo != null && setting.SettingInfo.Count > i
                    ? (setting.SettingInfo[i].Parameters ?? string.Empty).ToUpperInvariant()
                    : string.Empty;

                bool isLastParam = (i == totalParams - 1);

                if (i <= step)
                {
                    // Completed parameter (OK / PASS)
                    if (isLastParam)
                    {
                        tokens.Add(isFail ? "FAIL" : "PASS");
                    }
                    else if (pName.Contains("SHORT CIRCUIT"))
                    {
                        tokens.Add(isFail ? "FAULT" : "PASS");
                    }
                    else if (pName.Contains("FAN"))
                    {
                        tokens.Add("FAN:FAN:OK");
                    }
                    else if (pName.Contains("OVERLOAD"))
                    {
                        tokens.Add("115:115:PASS");
                    }
                    else if (pName.Contains("TIME"))
                    {
                        tokens.Add("368");
                    }
                    else if (pName.Contains("SR NO") || pName.Contains("SERIAL"))
                    {
                        tokens.Add(barcode);
                    }
                    else
                    {
                        tokens.Add("12.5:12.5:OK");
                    }
                }
                else
                {
                    // Pending parameter (ONGOING / LIVE)
                    if (isLastParam)
                    {
                        tokens.Add("ONGOING");
                    }
                    else if (pName.Contains("SHORT CIRCUIT") || pName.Contains("RESULT"))
                    {
                        tokens.Add("ONGOING");
                    }
                    else if (pName.Contains("FAN"))
                    {
                        tokens.Add("FAN:FAN:WAIT");
                    }
                    else if (pName.Contains("TIME"))
                    {
                        tokens.Add((step * 16).ToString());
                    }
                    else if (pName.Contains("SR NO") || pName.Contains("SERIAL"))
                    {
                        tokens.Add(barcode);
                    }
                    else
                    {
                        tokens.Add("0.0:0.0:ONGOING");
                    }
                }
            }

            tokens.Add("^");
            return string.Join(",", tokens);
        }
    }
}
#endif
