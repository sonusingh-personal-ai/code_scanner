using BusinessLogicLayer;
using Entity;
using Entity.Util;
using QRCoder; // Uses your project's existing QRCoder library
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO.Ports;
using System.Linq;
using System.Web.Mvc;
using Utility;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace CodeScanner.Controllers
{
    public class ComPortHelperController : Controller
    {
        // GET: ComPortHelper
        public ActionResult Index()
        {
            return View();
        }

        public enModel IsModelExists(string value)
        {
            var objENModel = new enModel() { Value = value };
            var objBLModel = new blModel(objENModel);
            try
            {
                objBLModel.Read();
            }
            catch (Exception ex)
            {
                Log.Error("ComPortHelper.CodeScanner.IsModelExists. Error while Read() Model. \n Exception : " + ex.ToString());
            }
            return objENModel;
        }

        public enSetting getSettingFile(string fileId)
        {
            var objENSetting = new enSetting() { FileId = fileId };
            var objBLSetting = new blSetting(objENSetting);
            try
            {
                objBLSetting.ReadAndAggregate(typeof(enSettingInfo), typeof(enModel));
            }
            catch (Exception ex)
            {
                throw;
            }
            return objENSetting;
        }

        public enSettingResponse CompairFile(enSetting setting, string[] response)
        {
            var SettingResp = new enSettingResponse();
            SettingResp.interType = new List<IntegerType>();
            if (response.Length < 5)
            {
                SettingResp.status = (int)ResponseStatus.Fail;
                SettingResp.message = "Response Parameter is missing";
                return SettingResp;
            }

            var objENModel = new enModel() { Value = setting.FileId };
            var objBLModel = new blModel(objENModel);
            try
            {
                objBLModel.Read();
            }
            catch (Exception ex)
            {
                throw;
            }

            SettingResp.model = objENModel.Name;
            SettingResp.header = response.First();
            SettingResp.footer = response.Last();
            SettingResp.displayPv = response[1];
            SettingResp.controlPv = response[2];
            SettingResp.sysRating = response[3];

            var indx = 0;
            var settingIndx = 0;
            var respLenght = response.Length;

            if ((respLenght - 5) == setting.SettingInfo.Count)
            {
                foreach (var item in response)
                {
                    if (indx > 3 && indx != (respLenght - 1))
                    {
                        var objSetting = setting.SettingInfo[settingIndx];
                        var integerVal = item.IndexOf(':');
                        if (integerVal >= 0)
                        {
                            string[] integerArray = item.Split(':');
                            SettingResp.interType.Add(new IntegerType
                            {
                                dispaly = integerArray[0],
                                actual = integerArray[1],
                                status = integerArray[2],
                                parameter = objSetting.Parameters
                            });
                        }
                        else
                        {
                            SettingResp.interType.Add(new IntegerType
                            {
                                status = item,
                                parameter = objSetting.Parameters
                            });
                        }
                        settingIndx++;
                    }
                    indx++;
                }
            }
            else
            {
                SettingResp.status = (int)ResponseStatus.Fail;
                SettingResp.message = "Response length mismatch";
                return SettingResp;
            }
            return SettingResp;
        }

        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Parity({0}):", defaultPortParity.ToString());
            parity = Enum.GetNames(typeof(Parity))[0];

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity);
        }

        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Data Bits({0}): ", defaultPortDataBits);
            dataBits = defaultPortDataBits.ToString();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits);
        }

        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available Stop Bits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Stop Bits({0}):", defaultPortStopBits.ToString());
            stopBits = "One";

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits);
        }

        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Handshake({0}):", defaultPortHandshake.ToString());
            handshake = defaultPortHandshake.ToString();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake);
        }

        public static List<enResponseSummary> SaveReponse(enSetting setting, string[] response, string barcode, bool isOk, int qcStatus, int visualby, int testedBy, int productionLine, int processEngg, string testingJig, string currentDate, string Time, bool barCodeStatus, string ConProgNo, string DisProgNo, string SysRating, int? printerModelId = null, int? line = null)
        {
            var objENResponse = new enResponse() { Barcode = barcode, QcStatus = qcStatus };
            var objBLResponse = new blResponse(objENResponse);
            var respId = 0;
            try
            {
                objBLResponse.Read();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            if (objENResponse.Id == 0)
            {
                objENResponse.QcStatus = qcStatus;
                objENResponse.VisualBy = visualby;
                objENResponse.TestedBy = testedBy;
                objENResponse.ProcessEngg = processEngg;
                objENResponse.ProductionLine = productionLine;
                objENResponse.SerialCardNo = testingJig;
                objENResponse.PrinterModelId = printerModelId;
                objENResponse.Line = line;
                objENResponse.ConProgNo = ConProgNo;
                objENResponse.DisProgNo = DisProgNo;
                objENResponse.SystemRating = SysRating;
                objENResponse.CurrentDate = currentDate;
                objENResponse.CurrentTime = Time;
                objENResponse.ResponseTime = DateTime.Now;
                objENResponse.Model = setting.Model.Name;

                Log.Info(" Model-TestingJig : " + testingJig);
                try
                {
                    respId = objBLResponse.Create();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                objENResponse.Id = respId;
            }

            var indx = 0;
            var j = 0;
            List<enResponseSummary> listOfResponseSummary = new List<enResponseSummary>();

            foreach (var item in response)
            {
                Log.Trace("Response Count " + j + "  :" + item);
                var IntergerTypeObject = new IntegerType();
                if (indx > 3 && indx != (response.Length - 1))
                {
                    Log.Trace("success Response Count " + j + "  :" + item);

                    var integerVal = item.IndexOf(':');
                    if (integerVal >= 0)
                    {
                        string[] integerArray = item.Split(':');
                        IntergerTypeObject.dispaly = integerArray[0];
                        IntergerTypeObject.actual = integerArray[1];
                        IntergerTypeObject.status = integerArray[2];
                        IntergerTypeObject.parameter = setting.SettingInfo[j].Parameters;
                    }
                    else
                    {
                        IntergerTypeObject.status = item;
                        IntergerTypeObject.parameter = setting.SettingInfo[j].Parameters;
                    }

                    var objENResponseSummary = new enResponseSummary() { ResponseId = objENResponse.Id, Parameters = IntergerTypeObject.parameter, Dispaly = IntergerTypeObject.dispaly, Actual = IntergerTypeObject.actual, Status = IntergerTypeObject.status, IsFinal = isOk };
                    var objBLResponseSummary = new blResponseSummary(objENResponseSummary);
                    try
                    {
                        objBLResponseSummary.Create();
                        listOfResponseSummary.Add(objENResponseSummary);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                    j++;
                }
                indx++;
            }

            return listOfResponseSummary;
        }

        public JsonResult CheckBarCode(string barcode, bool status, int qcStage)
        {
            if (!status)
            {
                var objENResponse = new enResponse() { Barcode = barcode, QcStatus = qcStage };
                var objBLResponse = new blResponse(objENResponse);
                try
                {
                    objBLResponse.Read();
                }
                catch (Exception ex)
                {
                    Log.Error("ComPortHelper.CodeScanner.CheckBarCode. Error while Read() Response. \n Exception : " + ex.ToString());
                    throw;
                }
                if (objENResponse.Id > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public Bitmap ConvertStringToImage(string qrCode)
        {
            // create a dummy Bitmap just to get the Graphics object
            Bitmap img = new Bitmap(1, 3);
            Graphics g = Graphics.FromImage(img);

            // The font for our text
            Font f = new Font("Arial", 16);

            // work out how big the text will be when drawn as an image
            SizeF size = g.MeasureString(qrCode, f);

            // create a new Bitmap of the required size
            img = new Bitmap(300, 250);

            g = Graphics.FromImage(img);

            // give it a white background
            g.Clear(Color.White);

            // draw the text in black
            g.DrawString(qrCode, f, Brushes.Black, 0, 0);

            return img;
            //var QrCodePath = Utility.ApplicationSettings.getQrCodePath;
            //QRCodeWriter.CreateQrCodeWithLogoImage("JKDJLJD", img, 250, 0).ChangeBarCodeColor(Color.SkyBlue).SaveAsPng(QrCodePath + "\\" + "QrCode" + "123" + ".png");

        }

        public static enSettingResponse generateLogs(int status, string message, string logMessage)
        {
            var request = new enSettingResponse();
            request.status = status;
            request.message = message;
            Log.Info(logMessage);

            return request;
        }

        #region print 1
        public void PrintQrCode(string barcode, int? printerModelId, string qrCodeText = null, string targetPrinterName = null)
        {
            try
            {
                // 1. Guard against machines with zero printers installed
                if (PrinterSettings.InstalledPrinters.Count == 0)
                {
                    Log.Info("[PrintQrCode] No printers installed on this system. Skipping physical label print.");
                    return;
                }

                string staticTestQrString =
                    "MODEL:Testing Data; TESTEDBY:kamal (fpsgn1639); CURRENTDATE:6/09/2026; " +
                    "DISP. PROG. NO.:GP_D_0.1; CONTROL PROG. NO.:GP_C_DSP_HR_0.9; " +
                    "BATTERY VOLT. : 25.5 || 25.6; OUTPUT VOLT. : 230 || 230; " +
                    "CHARGING CURRENT : 9.8 || 9.3; SOLAR VOLT. : 66.6 || 66.8; SOLAR CURRENT : 14.3 || 14.4";

                var qrCodeVal = !string.IsNullOrWhiteSpace(qrCodeText) ? qrCodeText : staticTestQrString;
                string cleanBarcode = !string.IsNullOrWhiteSpace(barcode) ? barcode : "GPSX16NHZ3K0B000004_1";

                List<string> modelValues = new List<string>
                {
                    qrCodeVal,                  // 0: Encoded inside the QR code matrix
                    printerModelId?.ToString(), // 1: Printer Model ID for left section
                    cleanBarcode                // 2: Serial/Barcode number for SERIAL NO. row
                };

                using (PrintDocument pd = CreateLabelPrintDocument(modelValues, targetPrinterName))
                {
                    if (pd == null)
                    {
                        Log.Info("[PrintQrCode] Print document could not be created or no valid printer available. Skipping print.");
                        return;
                    }

                    if (!pd.PrinterSettings.IsValid)
                    {
                        Log.Warn($"[PrintQrCode] Printer '{pd.PrinterSettings.PrinterName}' is not valid or offline. Skipping print.");
                        return;
                    }

                    // Never print to file-prompt virtual printers (e.g., Microsoft Print to PDF) as they hang web server processes waiting for modal file dialogs
                    string selectedPrinter = pd.PrinterSettings.PrinterName ?? string.Empty;
                    bool isPromptPrinter =
                        pd.PrinterSettings.PrintToFile ||
                        selectedPrinter.IndexOf("Print to PDF", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        selectedPrinter.IndexOf("XPS Document", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        selectedPrinter.IndexOf("OneNote", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        selectedPrinter.IndexOf("Fax", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        selectedPrinter.IndexOf("PORTPROMPT", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isPromptPrinter)
                    {
                        Log.Info($"[PrintQrCode] Printer '{selectedPrinter}' is a virtual prompt-to-file printer. Skipping physical print to prevent process hang.");
                        return;
                    }

                    Log.Info($"[PrintQrCode] Printing compact QR label to '{selectedPrinter}'...");
                    pd.Print();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[PrintQrCode] Error: {ex}");
            }
        }

        public Bitmap GenerateLabelBitmap(List<string> values, float dpi = 203.0f)
        {
            float totalWidthUnits = 197.0f; // 50mm
            float totalHeightUnits = 98.0f; // 25mm

            int pixelWidth = (int)Math.Round((totalWidthUnits / 100.0f) * dpi);
            int pixelHeight = (int)Math.Round((totalHeightUnits / 100.0f) * dpi);

            Bitmap bitmap = new Bitmap(pixelWidth, pixelHeight);
            bitmap.SetResolution(dpi, dpi);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.ScaleTransform(dpi / 100.0f, dpi / 100.0f);
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Default;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                RenderLabelPage(g, values);
            }

            return bitmap;
        }

        public bool IsPhysicalPrinterAvailable(string targetPrinterName = null)
        {
            try
            {
                if (PrinterSettings.InstalledPrinters.Count == 0)
                {
                    return false;
                }

                // If a specific printer was requested
                if (!string.IsNullOrWhiteSpace(targetPrinterName))
                {
                    bool exists = false;
                    foreach (string installed in PrinterSettings.InstalledPrinters)
                    {
                        if (string.Equals(installed, targetPrinterName.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists || IsVirtualPromptPrinter(targetPrinterName))
                    {
                        return false;
                    }

                    PrinterSettings ps = new PrinterSettings { PrinterName = targetPrinterName.Trim() };
                    return ps.IsValid;
                }

                // If no specific printer passed, check default printer
                PrinterSettings defaultSettings = new PrinterSettings();
                string defaultName = defaultSettings.PrinterName;

                if (!string.IsNullOrWhiteSpace(defaultName) && !IsVirtualPromptPrinter(defaultName) && defaultSettings.IsValid)
                {
                    return true;
                }

                // Check if ANY physical non-virtual installed printer exists
                foreach (string installed in PrinterSettings.InstalledPrinters)
                {
                    if (!IsVirtualPromptPrinter(installed))
                    {
                        PrinterSettings ps = new PrinterSettings { PrinterName = installed };
                        if (ps.IsValid) return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsVirtualPromptPrinter(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName)) return true;
            return printerName.IndexOf("Print to PDF", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   printerName.IndexOf("XPS Document", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   printerName.IndexOf("OneNote", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   printerName.IndexOf("Fax", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   printerName.IndexOf("PORTPROMPT", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private PrintDocument CreateLabelPrintDocument(List<string> values, string targetPrinterName = null)
        {
            try
            {
                if (PrinterSettings.InstalledPrinters.Count == 0)
                {
                    return null;
                }

                PrintDocument pd = new PrintDocument();
                PrinterSettings settings = new PrinterSettings();

                // If caller provided a specific target printer, verify it exists among installed printers
                string chosenPrinter = null;
                if (!string.IsNullOrWhiteSpace(targetPrinterName))
                {
                    foreach (string installed in PrinterSettings.InstalledPrinters)
                    {
                        if (string.Equals(installed, targetPrinterName.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            chosenPrinter = installed;
                            break;
                        }
                    }
                }

                // If no specific printer matched or was provided, fall back to default printer
                if (string.IsNullOrWhiteSpace(chosenPrinter))
                {
                    chosenPrinter = settings.PrinterName;
                }

                if (string.IsNullOrWhiteSpace(chosenPrinter))
                {
                    return null;
                }

                pd.PrinterSettings.PrinterName = chosenPrinter;

                if (!pd.PrinterSettings.IsValid)
                {
                    return null;
                }

                pd.DefaultPageSettings.Landscape = false;

                PaperSize labelSize = new PaperSize("50x25mm", 197, 98);
                try
                {
                    pd.DefaultPageSettings.PaperSize = labelSize;
                    pd.PrinterSettings.DefaultPageSettings.PaperSize = labelSize;
                }
                catch
                {
                    // Ignore printer drivers that reject custom paper sizes
                }

                pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                pd.OriginAtMargins = true;

                pd.PrintPage += (sender, args) =>
                {
                    // Set global crisp rendering hints for thermal print heads
                    Graphics g = args.Graphics;
                    g.SmoothingMode = SmoothingMode.None;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Default;
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    RenderLabelPage(g, values);
                    args.HasMorePages = false;
                };

                return pd;
            }
            catch (Exception ex)
            {
                Log.Error($"[CreateLabelPrintDocument] Error initializing print document: {ex.Message}");
                return null;
            }
        }

        private void RenderLabelPage(Graphics g, List<string> values)
        {
            string qrCodeContent = values[0];
            string printerModelId = values[1];

            // Properly extract barcodeValue from index 2
            string barcodeValue = (values.Count > 2 && !string.IsNullOrWhiteSpace(values[2])) ? values[2] : "GPSX16NDZ3K0B026418";

            float totalWidth = 197f;  // 50mm
            float totalHeight = 98f;  // 25mm

            // Balanced ratio: 122 units for left section, 75 units for right QR section
            // Moves divider left and eliminates empty gap between barcode and divider line
            float leftSectionWidth = 122f;
            float rightSectionWidth = totalWidth - leftSectionWidth; // 75f

            // 1. Draw Sharp Divider Line
            using (Pen linePen = new Pen(Color.Black, 1.0f))
            {
                g.DrawLine(linePen, leftSectionWidth, 0, leftSectionWidth, totalHeight);
            }

            // 2. Render High-Contrast Text & Serial Number
            DrawLeftSection(g, printerModelId, barcodeValue, leftSectionWidth, totalHeight);

            // 3. Render Crisp QR Code
            DrawRightQrCodeDirectly(g, qrCodeContent, leftSectionWidth, rightSectionWidth, totalHeight);
        }

        private void DrawLeftSection(Graphics g, string printerModelId, string barcodeValue, float width, float height)
        {
            if (string.IsNullOrEmpty(printerModelId)) return;

            string rawSerial = string.IsNullOrEmpty(barcodeValue) ? "GPSX16NHZ3K0B000004_1" : barcodeValue;
            string cleanSerial = rawSerial.Contains("_") ? rawSerial.Split('_')[0] : rawSerial;

            int modelId = Convert.ToInt32(printerModelId);
            var enPrinterModel = getPrinterModel(modelId);
            string modelName = enPrinterModel?.Name ?? string.Empty;

            // Barcode starts at 2.0 units, spanning 120.0 units (bars start at X=7.4, ending at X=116.6 before divider at 122.0)
            float startX = 2.0f;
            float textStartX = 6.0f;
            float barcodeWidth = 120.0f;

            var prevSmoothing = g.SmoothingMode;
            var prevPixelOffset = g.PixelOffsetMode;
            var prevInterpolation = g.InterpolationMode;
            var prevTextHint = g.TextRenderingHint;

            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Default;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (StringFormat leftFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming = StringTrimming.None
            })
            using (StringFormat centerFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming = StringTrimming.None
            })
            using (Font headerFont = new Font("Arial", 6.5f, FontStyle.Bold, GraphicsUnit.World))
            using (Font serialFont = new Font("Arial", 6.0f, FontStyle.Bold, GraphicsUnit.World))
            {
                // 1. MODEL : Name (aligned cleanly over barcode)
                float modelY = 10.0f;
                string modelDisplayText = string.IsNullOrEmpty(modelName) ? "MODEL :" : $"MODEL : {modelName}";
                g.DrawString(modelDisplayText, headerFont, brush, new RectangleF(textStartX, modelY, barcodeWidth, 8.0f), leftFormat);

                // 2. SERIAL NO. Header
                float serialHeaderY = 21.0f;
                g.DrawString("SERIAL NO.", headerFont, brush, new RectangleF(textStartX, serialHeaderY, barcodeWidth, 8.0f), leftFormat);

                // 3. Barcode (120 units wide, 42 units high, centered vertically)
                float barcodeY = 32.0f;
                float barcodeHeight = 42.0f;

                if (barcodeHeight > 5.0f && barcodeWidth > 10.0f)
                {
                    RenderBarcodeZXing(g, cleanSerial, startX, barcodeY, barcodeWidth, barcodeHeight);
                }

                // 4. Centered Serial Number Text Below Barcode
                float serialValueY = 77.0f;
                RectangleF serialValueRect = new RectangleF(startX, serialValueY, barcodeWidth, 8.5f);
                g.DrawString(cleanSerial, serialFont, brush, serialValueRect, centerFormat);
            }

            g.SmoothingMode = prevSmoothing;
            g.PixelOffsetMode = prevPixelOffset;
            g.InterpolationMode = prevInterpolation;
            g.TextRenderingHint = prevTextHint;
        }

        /// <summary>
        /// Renders Code 128 barcode natively via ZXing.Net at 1:1 printhead module resolution (244px).
        /// </summary>
        private void RenderBarcodeZXing(Graphics g, string barcodeValue, float x, float y, float width, float height)
        {
            if (string.IsNullOrEmpty(barcodeValue) || width <= 0f || height <= 0f) return;

            try
            {
                // Use fixed 244-pixel width (222 modules + 11px quiet zone on each side).
                // Prevents integer module downsampling artifacts that previously introduced a giant blank margin.
                int targetPixelWidth = 244;
                int targetPixelHeight = 85;

                BarcodeWriter writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = targetPixelWidth,
                        Height = targetPixelHeight,
                        Margin = 0,
                        PureBarcode = true
                    }
                };

                using (Bitmap barcodeImg = writer.Write(barcodeValue))
                {
                    g.DrawImage(barcodeImg, new RectangleF(x, y, width, height));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RenderBarcodeZXing] Error generating barcode: {ex.Message}");
            }
        }




        private void DrawRightQrCodeDirectly(Graphics g, string textToEncode, float leftOffset, float qrAreaWidth, float totalHeight)
        {
            if (string.IsNullOrEmpty(textToEncode)) return;

            try
            {
                // Clean breathing room (quiet zone distance) from divider line and outer borders
                float padding = 3.5f;
                float availableWidth = qrAreaWidth - (padding * 2f);
                float availableHeight = totalHeight - (padding * 2f);
                float qrSquareSize = Math.Min(availableWidth, availableHeight);

                float startX = leftOffset + ((qrAreaWidth - qrSquareSize) / 2f);
                float startY = (totalHeight - qrSquareSize) / 2f;

                // Match exact printhead / target DPI resolution
                float currentDpi = g.DpiX > 100f ? g.DpiX : 203f;
                int targetPixels = (int)Math.Round((qrSquareSize / 100f) * currentDpi);

                // Use ZXing BarcodeWriter to generate a 1-bit crisp binary QR matrix
                BarcodeWriter qrWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        CharacterSet = "UTF-8",
                        DisableECI = true,
                        ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.L,
                        Margin = 2, // Standard quiet zone (2 modules) around QR code so scanners reliably detect finder patterns
                        Width = targetPixels,
                        Height = targetPixels
                    }
                };

                using (Bitmap qrBitmap = qrWriter.Write(textToEncode))
                {
                    var prevInterpolation = g.InterpolationMode;
                    var prevSmoothing = g.SmoothingMode;
                    var prevPixelOffset = g.PixelOffsetMode;

                    // NearestNeighbor ensures binary 1-bit sharp dots for thermal printheads (no gray anti-aliasing blur/dither)
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.SmoothingMode = SmoothingMode.None;
                    g.PixelOffsetMode = PixelOffsetMode.Half;

                    g.DrawImage(qrBitmap, startX, startY, qrSquareSize, qrSquareSize);

                    g.InterpolationMode = prevInterpolation;
                    g.SmoothingMode = prevSmoothing;
                    g.PixelOffsetMode = prevPixelOffset;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[DrawRightQrCodeDirectly] Error generating QR code: {ex.Message}");
            }
        }

        private enPrinterModel getPrinterModel(int printerModelId)
        {
            var objENPrinterModel = new enPrinterModel() { Id = printerModelId };
            var objBLPrinterModel = new blPrinterModel(objENPrinterModel);
            try
            {
                objBLPrinterModel.Read();
            }
            catch (Exception ex)
            {
                Log.Error("[getPrinterModel] Error while Read() PrinterModel. \n Exception : " + ex.ToString());
            }

            //var objENPrinterModelValue = new enPrinterModelValue() { ModelId = printerModelId };
            //var objBLPrinterModelValue = new blPrinterModelValue(objENPrinterModelValue);
            //try
            //{
            //    objENPrinterModel.ModelValues = objBLPrinterModelValue.ReadAll();
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("[getPrinterModel] Error while Read() PrinterModelValue. \n Exception : " + ex.ToString());
            //    throw;
            //}

            return objENPrinterModel;
        }
        #endregion
    }
}
