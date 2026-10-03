using BusinessLogicLayer;
using Entity;
using Entity.Util;
using System.Drawing.Imaging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Utility;

namespace CodeScanner.Controllers
{
    public class ComPortController : ComPortHelperController
    {
        static SerialPort _serialPort;

        // How long to wait for a single line from the device before treating that
        // particular read as "missed". Without this, SerialPort.ReadLine() blocks
        // forever by default (ReadTimeout = -1), so one missed response from the
        // device hung this request - and the browser, since SendParameter is called
        // with a synchronous AJAX call - indefinitely.
        const int ReadPerLineTimeoutMs = 5000;

        // Total time (seconds) to keep waiting/retrying reads for a PASS/FAIL before
        // giving up on this scan entirely and returning a clear timeout result.
        const double OverallWatchdogSeconds = 120;
        // _serialPort is shared (static) across every request, and by design stays open
        // across separate HTTP requests while a scan's parameters stream in (see
        // IsRecurrence below). Without serializing access, two overlapping requests -
        // e.g. a rapid double-scan - can race: one closes/reopens the port while the
        // other is still mid-ReadLine() on it, producing "The port is closed".
        static readonly object _portLock = new object();

        private static void CloseSerialPort()
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error closing serial port: " + ex.Message);
                }
                try
                {
                    _serialPort.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error("Error disposing serial port: " + ex.Message);
                }
                _serialPort = null;
            }
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
                // Serializes all serial-port access across requests - see _portLock above.
                // If a second scan/recurrence call arrives while this one is still mid-read,
                // it now queues here instead of racing to close/reopen the same port.
                lock (_portLock)
                {
                    Log.Info("@#Recurrence  : " + objResponse.IsRecurrence);
                    if (!objResponse.IsRecurrence)
                    {
                        CloseSerialPort();

                        _serialPort = new SerialPort
                        {
                            PortName = objResponse.Port,
                            BaudRate = objResponse.BaudRate > 0 ? objResponse.BaudRate : 9600,
                            Parity = SetPortParity(Parity.None),
                            DataBits = SetPortDataBits(8),
                            StopBits = SetPortStopBits(StopBits.One),
                            Handshake = SetPortHandshake(Handshake.None),
                            ReadTimeout = ReadPerLineTimeoutMs
                        };
                        _serialPort.Open();
                        _serialPort.WriteLine("#" + objResponse.Barcode + "@");
                    }
                    else
                    {
                        if (_serialPort == null || !_serialPort.IsOpen)
                        {
                            CloseSerialPort();
                            matchString.status = (int)ResponseStatus.Fail;
                            matchString.message = "Serial port " + objResponse.Port + " is closed or disconnected. Please scan again.";
                            matchString.isOk = true;
                            return Json(matchString, JsonRequestBehavior.AllowGet);
                        }
                    }

                    List<List<string>> stringObject = new List<List<string>>();
                    DateTime now = DateTime.Now;
                    var t = 0d;
                    var count = 1;
                    #region while loop

                    while (t < OverallWatchdogSeconds)
                    {
                        // Recomputed every iteration (previously this was only ever calculated
                        // once, before the loop, and using TimeSpan.Seconds - which wraps back to
                        // 0 every 60 seconds - so the 120s watchdog below never actually fired and
                        // a genuinely silent device hung this request indefinitely).
                        t = DateTime.Now.Subtract(now).TotalSeconds;

                        if (_serialPort == null || !_serialPort.IsOpen)
                        {
                            CloseSerialPort();
                            matchString.status = (int)ResponseStatus.Fail;
                            matchString.message = "Serial port " + objResponse.Port + " was closed during read.";
                            matchString.isOk = true;
                            return Json(matchString, JsonRequestBehavior.AllowGet);
                        }

                        string rec;
                        try
                        {
                            rec = _serialPort.ReadLine();
                        }
                        catch (TimeoutException)
                        {
                            // Nothing arrived within ReadPerLineTimeoutMs - this is the "missed"
                            // case. Don't treat it as fatal by itself: log it and let the overall
                            // watchdog (t, checked at the top of this loop) decide whether to keep
                            // waiting for the device to catch up or give up for good.
                            Log.Error("ComPortController.SendParameter - Read timeout waiting for device response (elapsed " + t.ToString("F0") + "s / " + OverallWatchdogSeconds.ToString("F0") + "s).");
                            continue;
                        }

                        Log.Info("Receving string :- " + rec);

                        if (rec.Length != 4)
                        {
                            var isExist = rec.LastIndexOf("@");
                            var CarretIndx = rec.LastIndexOf("^");

                            if (isExist > -1 && CarretIndx > -1)
                            {
                                var nrec = rec.Substring(isExist, (CarretIndx + 1) - isExist);
                                Log.Info(nrec);
                                response = nrec.Split(',');

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
                                        CloseSerialPort();
                                        return Json(unResponsive, JsonRequestBehavior.AllowGet);
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
                                        if (matchStr == "FAIL")
                                        {
                                            Log.Info("****** RESULT FAIL ******");
                                            var resp = SaveReponse(setting, response, objResponse.Barcode, true, objResponse.QcStatus, objResponse.VisualBy, objResponse.TestedBy, objResponse.ProductionLine, objResponse.ProcessEngg, objResponse.SerialCardNo, objResponse.CurrentDate, objResponse.CurrentTime, true, ConProgNo, DisProgNo, SysRating, objResponse.PrinterModelId, objResponse.Line);
                                            matchString = CreateMatchResult(setting, response, stringObject);
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
                                            var resp = SaveReponse(setting, response, objResponse.Barcode, true, objResponse.QcStatus, objResponse.VisualBy, objResponse.TestedBy, objResponse.ProductionLine, objResponse.ProcessEngg, objResponse.SerialCardNo, objResponse.CurrentDate, objResponse.CurrentTime, false, ConProgNo, DisProgNo, SysRating, objResponse.PrinterModelId, objResponse.Line);

                                            matchString = CreateMatchResult(setting, response, stringObject);
                                            var QrCodeString = GenerateQrCodeString(objResponse, resp, listOfOfficeMembers, productinLine, matchString);
                                            SaveQrCodePng(QrCodeString, Color.Black, Path.Combine(QrCodePath, objResponse.Barcode + "_" + objENResponse.QcStatus + ".png"));

                                            CloseSerialPort();
                                            if (objResponse.ProductionLine == 2)
                                            {
                                                PrintQrCode(objENResponse.Barcode + "_" + objENResponse.QcStatus, objResponse.PrinterModelId, QrCodeString);
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
                                        matchString.model = setting.Model.Name;
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
                string displayPv = matchString?.displayPv ?? string.Empty;
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

        private enSettingResponse CreateMatchResult(enSetting setting, string[] response, List<List<string>> stringObject, bool isOk = true)
        {
            var matchString = CompairFile(setting, response);
            matchString.totalString = stringObject;
            matchString.SettingInfoList = setting.SettingInfo;
            matchString.model = setting.Model.Name;
            matchString.isOk = isOk;
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