using BusinessLogicLayer;
using Entity;
using Entity.Util;
using System.Drawing.Imaging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.SessionState;
using Utility;
using CodeScanner.Infrastructure;
#if DEBUG
using CodeScanner.Simulation;
#endif

namespace CodeScanner.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class ComPortController : ComPortHelperController
    {
        // Each blocking ReadLine() waits at most this long. Kept short so the loop can
        // notice pre-emption by a newer scan / a USB detach quickly; the overall budget
        // for a frame is still governed by OverallWatchdogSeconds.
        const int ReadSliceTimeoutMs = 1000;

        // Log a "still waiting" line at most this often (avoids 1 log/second spam).
        const int TimeoutLogIntervalSeconds = 5;

        // Total time (seconds) to keep waiting/retrying reads for a PASS/FAIL before
        // giving up on this scan entirely and returning a clear timeout result.
        const double OverallWatchdogSeconds = 15;

        // The physical port is shared across requests and, by design, stays open across
        // separate HTTP requests while a scan's parameters stream in (see IsRecurrence).
        // All lifecycle (open/close/retry/idle-release) and the cross-request lock now live
        // in SerialPortManager so close-then-reopen is deterministic.
        private static void CloseSerialPort(string reason = "scan finished")
        {
            SerialPortManager.Close(reason);
        }

        private JsonResult SupersededResult(enSettingResponse matchString)
        {
            matchString.status = (int)ResponseStatus.Fail;
            matchString.message = "Scan was superseded by a newer scan.";
            matchString.isOk = true;
            return Json(matchString, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SendParameter(enResponse objResponse)
        {
            var barcodeString = "";
            enSettingResponse matchString = new enSettingResponse();

            #region Check COM Port
            if (string.IsNullOrWhiteSpace(objResponse.Port))
            {
                var resp = generateLogs((int)ResponseStatus.Fail, "COM Port is not selected. Please select a valid COM Port.", "Stage 1. COM Port is missing");
                resp.isOk = true;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            #endregion

            #region Check QrCodePath 
            var QrCodePath = ApplicationSettings.getQrCodePath;
            if (!Directory.Exists(QrCodePath))
            {
                var resp = generateLogs((int)ResponseStatus.Fail, "Directory not exist. \n Path : " + QrCodePath, "Stage 2. QrCodePath not exist");
                resp.isOk = true;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            #endregion

            #region Check Barcode Length
            if (objResponse.Barcode != null && objResponse.Barcode.Length >= 10 && objResponse.Barcode.Length <= 25)
            {
                string IsThree = objResponse.Barcode.Substring(0, 1);
                barcodeString = IsThree == "3" ? objResponse.Barcode.Substring(3, 3) : objResponse.Barcode.Substring(0, 3);
            }
            #endregion

            #region checkString Not null
            if (barcodeString == "")
            {
                var resp = generateLogs((int)ResponseStatus.Fail, "Barcode string length is Zero", "Stage 3. Barcode string length is Zero");
                resp.isOk = true;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            #endregion

            #region Check Model Exist
            var model = IsModelExists(barcodeString);
            if (model.Id == 0)
            {
                var resp = generateLogs((int)ResponseStatus.Fail, "Model does not exist.", "Stage 4. Model does not exist.");
                resp.isOk = true;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            #endregion

            var objENResponse = new enResponse() { Barcode = objResponse.Barcode, QcStatus = objResponse.QcStatus };
            var objBLResponse = new blResponse(objENResponse);
            try
            {
                objBLResponse.Read();
            }
            catch (Exception ex)
            {
                Log.Error("COMPostController.SendParameter Error while read Response() \n Exception : " + ex.ToString());
            }

            if (objResponse.IsRepeat)
            {
                try
                {
                    objBLResponse.Delete();
                }
                catch (Exception ex)
                {
                    Log.Error("COMPostController.SendParameter Error while Delete Response() /n Exp: " + ex);
                }

                var objENResponseSummary = new enResponseSummary() { ResponseId = objENResponse.Id };
                var objBLResponseSummary = new blResponseSummary(objENResponseSummary);
                try
                {
                    objBLResponseSummary.Delete();
                }
                catch (Exception ex)
                {
                    Log.Error("COMPostController.SendParameter Error while Delete ResponseSummary() /n Exp: " + ex.ToString());
                }
            }

            var setting = getSettingFile(barcodeString);
            if (setting.Id == 0)
            {
                var resp = generateLogs((int)ResponseStatus.Fail, "Setting file is missing", "Stage 5. Setting file is missing");
                resp.isOk = true;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }

            string[] response = new string[] { };
            try
            {
                // A brand-new scan bumps the generation BEFORE waiting on the lock, so any
                // in-flight read from the previous scan notices within ~1s and bails out
                // instead of making this scan queue for the full watchdog period.
                long scanGeneration = objResponse.IsRecurrence
                    ? SerialPortManager.CurrentGeneration
                    : SerialPortManager.BeginNewScan();

                string portName = SerialPortManager.NormalizePortName(objResponse.Port);
                int baudRate = objResponse.BaudRate > 0 ? objResponse.BaudRate : 9600;

                // Serializes all serial-port access across requests (open/write/read/close).
                lock (SerialPortManager.PortLock)
                {
                    Log.Info("@#Recurrence  : " + objResponse.IsRecurrence + " | Port: " + portName + " | Gen: " + scanGeneration);
#if DEBUG
                    bool isSim = string.Equals(objResponse.Port, "SIMULATOR", StringComparison.OrdinalIgnoreCase);
#else
                    const bool isSim = false;
#endif
                    if (!isSim)
                    {
                        if (scanGeneration != SerialPortManager.CurrentGeneration)
                        {
                            return SupersededResult(matchString);
                        }

                        if (!objResponse.IsRecurrence)
                        {
                            // Deterministic close -> (retrying) open -> purge stale bytes -> send.
                            SerialPortManager.Open(portName, baudRate, ReadSliceTimeoutMs);
                            SerialPortManager.WriteLine("#" + objResponse.Barcode + "@");
                        }
                        else if (!SerialPortManager.IsOpen)
                        {
                            CloseSerialPort("recurrence found port closed");
                            matchString.status = (int)ResponseStatus.Fail;
                            matchString.message = "Serial port " + portName + " is closed or disconnected. Please scan again.";
                            matchString.isOk = true;
                            return Json(matchString, JsonRequestBehavior.AllowGet);
                        }
                    }

                    List<List<string>> stringObject = new List<List<string>>();
                    DateTime now = DateTime.Now;
                    DateTime lastTimeoutLog = DateTime.MinValue;
                    var t = 0d;
                    var count = 1;
                    var reconnectsUsed = 0;
                    const int MaxReconnectsPerRequest = 2;
                    #region while loop

                    while (t < OverallWatchdogSeconds)
                    {
                        t = DateTime.Now.Subtract(now).TotalSeconds;

                        if (!isSim)
                        {
                            // A newer scan has taken over the port - stop without touching it.
                            if (scanGeneration != SerialPortManager.CurrentGeneration)
                            {
                                return SupersededResult(matchString);
                            }

                            if (!SerialPortManager.IsOpen)
                            {
                                if (SerialPortManager.IsPortPresent(portName) && reconnectsUsed < MaxReconnectsPerRequest)
                                {
                                    reconnectsUsed++;
                                    Log.Warn("[SendParameter] Port " + portName + " closed unexpectedly but device is still attached. Reconnecting (" + reconnectsUsed + "/" + MaxReconnectsPerRequest + ").");
                                    SerialPortManager.Open(portName, baudRate, ReadSliceTimeoutMs);
                                    continue;
                                }

                                CloseSerialPort("closed during read");
                                matchString.status = (int)ResponseStatus.Fail;
                                matchString.message = "Serial port " + portName + " was closed during read.";
                                matchString.isOk = true;
                                return Json(matchString, JsonRequestBehavior.AllowGet);
                            }
                        }

                        string rec;
#if DEBUG
                        if (isSim)
                        {
                            bool isFailSim = objResponse.Barcode != null && objResponse.Barcode.IndexOf("FAIL", StringComparison.OrdinalIgnoreCase) >= 0;
                            rec = JigSimulationEngine.GetNextFrame(setting, objResponse.Barcode, objResponse.IsRecurrence, isFailSim);
                        }
                        else
#endif
                        {
                            try
                            {
                                rec = SerialPortManager.Port.ReadLine();
                                SerialPortManager.Touch();
                            }
                            catch (TimeoutException)
                            {
                                // Only now (no data for a slice) do we pay for a device-presence check.
                                if (!SerialPortManager.IsPortPresent(portName))
                                {
                                    CloseSerialPort("device detached");
                                    matchString.status = (int)ResponseStatus.Fail;
                                    matchString.message = $"COM Port '{portName}' was disconnected. Please check the cable.";
                                    matchString.isOk = true;
                                    Log.Warn($"[SendParameter] Port '{portName}' disconnected during read.");
                                    return Json(matchString, JsonRequestBehavior.AllowGet);
                                }

                                if ((DateTime.Now - lastTimeoutLog).TotalSeconds >= TimeoutLogIntervalSeconds)
                                {
                                    lastTimeoutLog = DateTime.Now;
                                    Log.Warn("ComPortController.SendParameter - Waiting for device response on " + portName + " (elapsed " + t.ToString("F0") + "s / " + OverallWatchdogSeconds.ToString("F0") + "s).");
                                }
                                continue;
                            }
                            catch (Exception readEx) when (readEx is IOException || readEx is UnauthorizedAccessException || readEx is InvalidOperationException || readEx is ObjectDisposedException)
                            {
                                // Transient driver hiccup while the cable is still attached: reconnect in place
                                // and keep listening (the jig is already running the test - do NOT resend).
                                if (SerialPortManager.IsPortPresent(portName) && reconnectsUsed < MaxReconnectsPerRequest)
                                {
                                    reconnectsUsed++;
                                    Log.Warn("[SendParameter] Read error on " + portName + " while device still attached (" + readEx.GetType().Name + ": " + readEx.Message + "). Reconnecting (" + reconnectsUsed + "/" + MaxReconnectsPerRequest + ").");
                                    try
                                    {
                                        SerialPortManager.Open(portName, baudRate, ReadSliceTimeoutMs);
                                        continue;
                                    }
                                    catch (Exception reopenEx)
                                    {
                                        Log.Error("[SendParameter] Reconnect to " + portName + " failed: " + reopenEx.Message);
                                    }
                                }

                                CloseSerialPort("read error");
                                matchString.status = (int)ResponseStatus.Fail;
                                matchString.message = SerialPortManager.IsPortPresent(portName)
                                    ? $"COM Port '{portName}' communication error: {readEx.Message}. Please scan again."
                                    : $"COM Port '{portName}' was disconnected. Please check the cable.";
                                matchString.isOk = true;
                                Log.Error("ComPortController.SendParameter read failure: " + readEx);
                                return Json(matchString, JsonRequestBehavior.AllowGet);
                            }
                        }

                        Log.Info("Receving string :- " + rec);

                        if (!string.IsNullOrWhiteSpace(rec))
                        {
                            // Robust frame locator: locate the latest complete @...^ frame in the buffer
                            int end = rec.LastIndexOf('^');
                            int start = (end > 0) ? rec.LastIndexOf('@', end) : -1;

                            if (start >= 0 && end > start)
                            {
                                var nrec = rec.Substring(start, (end + 1) - start);
                                Log.Info(nrec);
                                response = nrec.Split(',').Select(tok => tok.Trim()).ToArray();

                                // A partial/noisy line (e.g. port opened mid-transmission, line glitch)
                                // must not abort the scan with IndexOutOfRange - skip it and keep listening.
                                if (response.Length < 5)
                                {
                                    Log.Warn("[SendParameter] Ignoring malformed frame (" + response.Length + " fields): " + nrec);
                                    continue;
                                }

                                Log.Info("response 1 : " + response[1]);
                                Log.Info("dpn : " + objResponse.DisProgNo);

                                //if (objResponse.DisProgNo != null)
                                //{
                                //    response[1] = objResponse.DisProgNo;
                                //}

                                Log.Info("response 2 : " + response[1]);

                                var DisProgNo = objResponse.DisProgNo;

                                var ConProgNo = response[2];
                                var SysRating = response[3];

                                if (response.Length > 3)
                                {
                                    Log.Info("Length");
                                    Log.Info(response.Length.ToString());
                                    var matchStr = response[response.Length - 2];
                                    var unResponsive = response[response.Length - 3];

                                    if (unResponsive == "SCAN CODE")
                                    {
                                        CloseSerialPort("device requested re-scan");
                                        matchString.status = (int)ResponseStatus.Fail;
                                        matchString.message = "Device requested a re-scan (SCAN CODE). Please scan the barcode again.";
                                        matchString.isOk = true;
                                        return Json(matchString, JsonRequestBehavior.AllowGet);
                                    }

                                    Log.Info(matchStr);

                                    if (matchStr == "FAIL" || matchStr == "PASS")
                                    {
                                        Log.Info("****** Final Result ******");
                                        Log.Info("****** " + matchStr + " ******");
                                        Log.Error("**** Process : " + count + "  || END ");
                                        var objENOfficeMember = new enOfficeMember();
                                        var objBLOfficeMember = new blOfficeMember(objENOfficeMember);
                                        List<enOfficeMember> listOfOfficeMembers = new List<enOfficeMember>();
                                        try
                                        {
                                            listOfOfficeMembers = objBLOfficeMember.ReadAll();
                                        }
                                        catch (Exception ex)
                                        {
                                            throw;
                                        }

                                        Log.Info(listOfOfficeMembers.Count.ToString());
                                        var productinLine = objResponse.ProductionLine == 1 ? "Card" : "Assembly";
                                        t = OverallWatchdogSeconds + 1;

                                        string resolvedDisProgNo = GetDisplayProgramNumber(listOfOfficeMembers, objResponse.DisProgNo, response[1]);

                                        if (matchStr == "FAIL")
                                        {
                                            Log.Info("****** RESULT FAIL ******");
                                            var resp = SaveReponse(setting, response, objResponse.Barcode, true, objResponse.QcStatus, objResponse.VisualBy, objResponse.TestedBy, objResponse.ProductionLine, objResponse.ProcessEngg, objResponse.SerialCardNo, objResponse.CurrentDate, objResponse.CurrentTime, true, ConProgNo, resolvedDisProgNo, SysRating, objResponse.PrinterModelId, objResponse.Line);
                                            matchString = CreateMatchResult(setting, response, stringObject, true, nrec);
                                            matchString.displayPv = resolvedDisProgNo;
                                            matchString.status = (int)ResponseStatus.Fail;
                                            var QrCodeString = GenerateQrCodeString(objResponse, resp, listOfOfficeMembers, productinLine, matchString);
                                            SaveQrCodePng(QrCodeString, Color.OrangeRed, Path.Combine(QrCodePath, objENResponse.Barcode + "_" + objENResponse.QcStatus + ".png"));

                                            CloseSerialPort();
                                            return Json(matchString, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            Log.Info("##Setting : " + setting);
                                            Log.Info("##Response : " + response);

                                            Log.Info("****** RESULT PASS ******");
                                            Log.Info("Model Value \n" + objResponse.VisualBy + " " + objResponse.TestedBy + " " + objResponse.ProductionLine + " " + objResponse.ProcessEngg);
                                            var resp = SaveReponse(setting, response, objResponse.Barcode, true, objResponse.QcStatus, objResponse.VisualBy, objResponse.TestedBy, objResponse.ProductionLine, objResponse.ProcessEngg, objResponse.SerialCardNo, objResponse.CurrentDate, objResponse.CurrentTime, false, ConProgNo, resolvedDisProgNo, SysRating, objResponse.PrinterModelId, objResponse.Line);

                                            matchString = CreateMatchResult(setting, response, stringObject, true, nrec);
                                            matchString.displayPv = resolvedDisProgNo;
                                            matchString.status = (int)ResponseStatus.Pass;
                                            var QrCodeString = GenerateQrCodeString(objResponse, resp, listOfOfficeMembers, productinLine, matchString);
                                            SaveQrCodePng(QrCodeString, Color.Black, Path.Combine(QrCodePath, objResponse.Barcode + "_" + objENResponse.QcStatus + ".png"));

                                            CloseSerialPort();
                                            if (objResponse.ProductionLine == 2)
                                            {
                                                string printBarcode = $"{objENResponse.Barcode}_{objENResponse.QcStatus}";
                                                int? printModelId = objResponse.PrinterModelId;
                                                string printQrText = QrCodeString;
                                                string printTargetName = objResponse.PrinterName;

                                                bool isPrinterAttached = IsPhysicalPrinterAvailable(printTargetName);
                                                if (isPrinterAttached)
                                                {
                                                    ThreadPool.QueueUserWorkItem(_ =>
                                                    {
                                                        try
                                                        {
                                                            PrintQrCode(printBarcode, printModelId, printQrText, printTargetName);
                                                        }
                                                        catch (Exception exPrint)
                                                        {
                                                            Log.Error($"[SendParameter] Background print error: {exPrint.Message}");
                                                        }
                                                    });
                                                }
                                                else
                                                {
                                                    matchString.printerStatus = "No printer connected";
                                                    Log.Info("[SendParameter] No physical printer connected. Skipping print dispatch.");
                                                }
                                            }
                                            return Json(matchString, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        Log.Error("**** Process Start : " + count + "  || request ");
                                        count++;
                                        stringObject.Add(response.ToList());
                                        matchString = CompairFile(setting, response);
                                        matchString.totalString = stringObject;
                                        matchString.SettingInfoList = setting.SettingInfo;
                                        matchString.model = setting.Model != null ? setting.Model.Name : string.Empty;
                                        matchString.rawData = nrec;

                                        // If an intermediate frame failed parameter mapping (e.g. transient line glitch),
                                        // do NOT abort the scan or show an error alert to the operator. Keep listening for the next complete frame!
                                        if (!string.IsNullOrEmpty(matchString.message))
                                        {
                                            Log.Warn("[SendParameter] Intermediate frame skipped (" + matchString.message + "). Waiting for next complete frame.");
                                            continue;
                                        }

                                        return Json(matchString, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }

                    }
                    #endregion

                    // Fell out of the loop because OverallWatchdogSeconds elapsed without a
                    // PASS/FAIL - return a clear timeout result instead of hanging or handing
                    // back an empty/ambiguous object. isOk = true so the client's auto-retry
                    // (SendToComPort) stops instead of looping forever against a dead device.
                    Log.Error("ComPortController.SendParameter - Overall watchdog (" + OverallWatchdogSeconds.ToString("F0") + "s) elapsed without PASS/FAIL for Barcode=" + objResponse.Barcode);
                    CloseSerialPort();
                    matchString.status = (int)ResponseStatus.Fail;
                    matchString.message = "No response received from the device within " + OverallWatchdogSeconds.ToString("F0") + " seconds. Check the connection and try again.";
                    matchString.isOk = true;
                    return Json(matchString, JsonRequestBehavior.AllowGet);
                } // end lock (_portLock)
            }
            catch (Exception ex)
            {
                CloseSerialPort();
                matchString.status = (int)ResponseStatus.Fail;
                matchString.message = ex.Message;
                matchString.isOk = true;
                Log.Error("Exception : " + ex.ToString());
                return Json(matchString, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Print(string code, int printerModelId)
        {
            PrintQrCode(code, printerModelId);
            return Json(new { status = 1, message = "Print" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult PreviewLabel(string code, int printerModelId, string qrText = null, float dpi = 300.0f)
        {
            try
            {
                string staticTestQrString =
                    "MODEL:Testing Data; TESTEDBY:kamal (fpsgn1639); CURRENTDATE:6/09/2026; " +
                    "DISP. PROG. NO.:GP_D_0.1; CONTROL PROG. NO.:GP_C_DSP_HR_0.9; " +
                    "BATTERY VOLT. : 25.5 || 25.6; OUTPUT VOLT. : 230 || 230; " +
                    "CHARGING CURRENT : 9.8 || 9.3; SOLAR VOLT. : 66.6 || 66.8; SOLAR CURRENT : 14.3 || 14.4";

                string qrString = !string.IsNullOrWhiteSpace(qrText) ? qrText : staticTestQrString;
                string cleanBarcode = !string.IsNullOrWhiteSpace(code) ? code : "GPSX16NHZ3K0B000004_1";

                List<string> modelValues = new List<string>
                {
                    qrString,
                    printerModelId.ToString(),
                    cleanBarcode
                };

                float renderDpi = dpi > 50.0f ? dpi : 300.0f;
                using (Bitmap bmp = GenerateLabelBitmap(modelValues, renderDpi))
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    return File(ms.ToArray(), "image/png");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[PreviewLabel] Error: {ex}");
                return Content("Error generating label preview: " + ex.Message);
            }
        }

        public string GenerateQrCodeString(enResponse objResponse, List<enResponseSummary> listofResponses, List<enOfficeMember> listOfOfficeMembers, string productionLine, enSettingResponse matchString)
        {
            try
            {
                if (objResponse == null)
                {
                    Log.Warn("GenerateQrCodeString called with null objResponse.");
                    return string.Empty;
                }

                string testedByName = GetMemberName(listOfOfficeMembers, objResponse.TestedBy);
                string displayPv = GetDisplayProgramNumber(listOfOfficeMembers, objResponse.DisProgNo, matchString?.displayPv);
                string controlPv = matchString?.controlPv ?? string.Empty;

                // Populate directly from original dynamic objects (No static/testing strings)
                List<string> parts = new List<string>
                {
                    $"MODEL:{matchString?.model ?? string.Empty}",
                    $"TESTEDBY:{testedByName ?? string.Empty}",
                    $"CURRENTDATE:{objResponse.CurrentDate ?? string.Empty}",
                    $"DISP. PROG. NO.:{displayPv}",
                    $"CONTROL PROG. NO.:{controlPv}"
                };

                if (listofResponses != null)
                {
                    foreach (var summary in listofResponses)
                    {
                        if (summary == null) continue;

                        string cleanParam = summary.Parameters?.Trim().ToUpper() ?? string.Empty;
                        string display = summary.Dispaly?.Trim() ?? string.Empty;
                        string actual = summary.Actual?.Trim() ?? string.Empty;
                        string cleanStatus = $"{display} || {actual}";

                        switch (cleanParam)
                        {
                            case "BATTERY VOLTAGE":
                            case "BATTERY VOLT.":
                                parts.Add($"BATTERY VOLT. : {cleanStatus}");
                                break;
                            case "OUTPUT VOLTAGE":
                            case "OUTPUT VOLT.":
                                parts.Add($"OUTPUT VOLT. : {cleanStatus}");
                                break;
                            case "CHARGING CURRENT":
                                parts.Add($"CHARGING CURRENT : {cleanStatus}");
                                break;
                            case "SOLAR VOLTAGE":
                            case "SOLAR VOLT.":
                                parts.Add($"SOLAR VOLT. : {cleanStatus}");
                                break;
                            case "SOLAR CURRENT":
                                parts.Add($"SOLAR CURRENT : {cleanStatus}");
                                break;
                        }
                    }
                }

                // Single-line semicolon output guarantees high QR scannability on thermal labels
                string finalQrString = string.Join("; ", parts);
                Log.Info($"PRODUCTION QR OUTPUT: {finalQrString}");

                return finalQrString;
            }
            catch (Exception ex)
            {
                Log.Error($"Error in GenerateQrCodeString: {ex.Message}");
                throw;
            }
        }
        // Helper method to safely lookup names and avoid NullReferenceException
        private static string GetMemberName(List<enOfficeMember> members, int? memberId)
        {
            if (members == null || !memberId.HasValue) return string.Empty;

            var member = members.FirstOrDefault(x => x.ID == memberId.Value);
            return member != null ? member.Name : string.Empty;
        }

        // Helper method to resolve Display Program Number from office members or fallback to device response
        private static string GetDisplayProgramNumber(List<enOfficeMember> members, string disProgNo, string fallbackFromDevice)
        {
            if (!string.IsNullOrWhiteSpace(disProgNo))
            {
                int memberId;
                if (int.TryParse(disProgNo, out memberId))
                {
                    var member = members?.FirstOrDefault(x => x.ID == memberId && x.Type == (int)Utility.OfficeMember.ProgDisNo);
                    if (member == null)
                    {
                        member = members?.FirstOrDefault(x => x.ID == memberId);
                    }
                    if (member != null && !string.IsNullOrWhiteSpace(member.Name))
                    {
                        return member.Name.Trim();
                    }
                }
                else
                {
                    return disProgNo.Trim();
                }
            }

            return fallbackFromDevice?.Trim() ?? string.Empty;
        }

        private enSettingResponse CreateMatchResult(enSetting setting, string[] response, List<List<string>> stringObject, bool isOk = true, string rawData = null)
        {
            var matchString = CompairFile(setting, response);
            matchString.totalString = stringObject;
            matchString.SettingInfoList = setting.SettingInfo;
            matchString.model = setting.Model != null ? setting.Model.Name : (setting.FileId ?? string.Empty);
            matchString.isOk = isOk;
            matchString.rawData = rawData;
            return matchString;
        }

        private static void SaveQrCodePng(string qrCodeText, Color qrColor, string filePath)
        {
            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.M))
                using (QRCode qrCode = new QRCode(qrData))
                using (Bitmap qrBitmap = qrCode.GetGraphic(20, qrColor, Color.White, true))
                {
                    qrBitmap.Save(filePath, ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[SaveQrCodePng] Failed to save QR image to {filePath}: {ex}");
            }
        }

    }
}