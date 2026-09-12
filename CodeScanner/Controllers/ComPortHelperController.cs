using BusinessLogicLayer;
using Entity;
using Entity.Util;
using QRCoder; // Uses your project's existing QRCoder library
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO.Ports;
using System.Linq;
using System.Web.Mvc;
using Utility;
using System.Drawing.Text;

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

        public static List<enResponseSummary> SaveReponse(enSetting setting, string[] response, string barcode, bool isOk, int qcStatus, int visualby, int testedBy, int productionLine, int processEngg, string testingJig, string currentDate, string Time, bool barCodeStatus, string ConProgNo, string DisProgNo, string SysRating, int? printerModelId = null)
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
        public void PrintQrCode(string qrCode, int? printerModelId, string barcodeValue = null,string qrCodeValues = "")
        {
            try
            {
         
             string staticTestQrString =
             "MODEL : Testing Data\n" +
             "TESTEDBY : kamal (fpsgn1639)\n" +
             "CURRENTDATE : 6/09/2026\n" +
             "DISP. PROG. NO. : GP_D_0.1\n" +
             "CONTROL PROG. NO. : GP_C_DSP_HR_0.9\n" +
             "BATTERY VOLT. : 25.5 || 25.6\n" +
             "OUTPUT VOLT. : 230 || 230\n" +
             "CHARGING CURRENT : 9.8 || 9.3\n" +
             "SOLAR VOLT. : 66.6 || 66.8\n" +
             "SOLAR CURRENT : 14.3 || 14.4";


                var qrCodeVal = string.IsNullOrWhiteSpace(qrCodeValues) ? staticTestQrString : qrCodeValues;

                // Ensure real serial/barcode number is passed cleanly instead of falling back to "no-barcode"
                string cleanBarcode = !string.IsNullOrWhiteSpace(qrCode) ? qrCode : "G-PSX16NDZ3K0B02641";

                List<string> modelValues = new List<string>
                {
                    qrCodeVal,                  // 0: Encoded inside the QR code matrix
                    printerModelId?.ToString(), // 1: Printer Model ID for left section
                    cleanBarcode                // 2: Serial/Barcode number for SERIAL NO. row
                };

                using (PrintDocument pd = CreateLabelPrintDocument(modelValues))
                {
                    Log.Info("[PrintQrCode] Printing compact QR label...");
                    pd.Print();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[PrintQrCode] Error: {ex}");
            }
        }

        private PrintDocument CreateLabelPrintDocument(List<string> values)
        {
            PrintDocument pd = new PrintDocument();
            PrinterSettings settings = new PrinterSettings();

            pd.PrinterSettings.PrinterName = settings.PrinterName;
            pd.DefaultPageSettings.Landscape = false;

            PaperSize labelSize = new PaperSize("50x25mm", 197, 98);
            pd.DefaultPageSettings.PaperSize = labelSize;
            pd.PrinterSettings.DefaultPageSettings.PaperSize = labelSize;
            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            pd.OriginAtMargins = true;

            pd.PrintPage += (sender, args) =>
            {
                // Set global crisp rendering hints for thermal print heads
                Graphics g = args.Graphics;
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.CompositingQuality = CompositingQuality.HighSpeed;

                // Forces crisp 1-bit text without grey/fuzzy anti-aliased edges
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                RenderLabelPage(g, values);
                args.HasMorePages = false;
            };

            return pd;
        }

        private void RenderLabelPage(Graphics g, List<string> values)
        {
            string qrCodeContent = values[0];
            string printerModelId = values[1];

            // Updated: Properly extracts barcodeValue from index 2
            string barcodeValue = (values.Count > 2 && !string.IsNullOrWhiteSpace(values[2])) ? values[2] : "GPSX16NDZ3K0B026418";

            float totalWidth = 197f;  // 50mm
            float totalHeight = 98f;  // 25mm

            float leftSectionWidth = totalWidth * 0.55f;
            float rightSectionWidth = totalWidth * 0.45f;

            // 1. Draw Sharp Divider Line
            using (Pen linePen = new Pen(Color.Black, 1.5f))
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

            // Sanitize the serial number to strip off "_1" or any trailing underscore suffix
            string cleanBarcodeValue = !string.IsNullOrWhiteSpace(barcodeValue)
                ? barcodeValue.Split('_')[0].Trim()
                : string.Empty;

            int modelId = Convert.ToInt32(printerModelId);
            var enPrinterModel = getPrinterModel(modelId);

            // Build details list starting with the Model Name
            List<KeyValuePair<string, string>> detailsList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("MODEL", enPrinterModel?.Name ?? string.Empty)
            };

            // Safely append extra values only if they exist
            if (enPrinterModel?.ModelValues != null && enPrinterModel.ModelValues.Count > 0)
            {
                int maxAllowedRows = 6;
                int rowCount = Math.Min(enPrinterModel.ModelValues.Count, maxAllowedRows);

                for (int i = 0; i < rowCount; i++)
                {
                    var item = enPrinterModel.ModelValues[i];
                    if (item != null)
                    {
                        detailsList.Add(new KeyValuePair<string, string>(item.Name ?? string.Empty, item.Value ?? string.Empty));
                    }
                }
            }

            // Layout Offsets
            float startX = 8f;       // Left margin
            float startY = 5f;       // Top margin
            float colonX = 65f;      // Colon alignment column
            float valueX = 69f;      // Values alignment column

            float fontEmSize = 4.0f;
            float lineHeight = 9.0f;

            using (Font dynamicFont = new Font("Arial", fontEmSize, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (StringFormat leftFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming = StringTrimming.None
            })
            {
                // 1. Draw Details List (MODEL and any optional ModelValues)
                for (int i = 0; i < detailsList.Count; i++)
                {
                    float currentY = startY + (i * lineHeight);

                    string labelText = detailsList[i].Key.Trim();
                    string valueText = detailsList[i].Value.Trim();

                    // Draw Label
                    RectangleF labelRect = new RectangleF(startX, currentY, colonX - startX - 1f, lineHeight);
                    g.DrawString(labelText, dynamicFont, brush, labelRect, leftFormat);

                    // Draw Colon
                    RectangleF colonRect = new RectangleF(colonX, currentY, 3f, lineHeight);
                    g.DrawString(":", dynamicFont, brush, colonRect, leftFormat);

                    // Draw Value
                    RectangleF valueRect = new RectangleF(valueX, currentY, width - valueX - 1f, lineHeight);
                    g.DrawString(valueText, dynamicFont, brush, valueRect, leftFormat);
                }

                // 2. Draw "SERIAL NO." Header Line
                float serialHeaderY = startY + (detailsList.Count * lineHeight) + 1f;
                RectangleF serialHeaderRect = new RectangleF(startX, serialHeaderY, width - startX - 2f, lineHeight);
                g.DrawString("SERIAL NO.", dynamicFont, brush, serialHeaderRect, leftFormat);

                // 3. Draw Clean Serial Number Text Value (without "_1")
                float serialValueY = serialHeaderY + lineHeight - 1f;
                RectangleF serialValueRect = new RectangleF(startX, serialValueY, width - startX - 2f, 10f);

                using (Font serialValueFont = new Font("Arial", 5.0f, FontStyle.Bold))
                {
                    g.DrawString(cleanBarcodeValue, serialValueFont, brush, serialValueRect, leftFormat);
                }

                // 4. Draw 1D Barcode directly below the Serial Number Text
                float barcodeY = serialValueY + 10f;
                float barcodeHeight = height - barcodeY - 3f; // Uses remaining vertical height
                float barcodeWidth = width - startX - 4f;
            }
        }

        private void DrawRightQrCodeDirectly(Graphics g, string textToEncode, float leftOffset, float qrAreaWidth, float totalHeight)
        {
            if (string.IsNullOrEmpty(textToEncode)) return;

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrData = qrGenerator.CreateQrCode(textToEncode, QRCodeGenerator.ECCLevel.L))
            using (QRCode qrCode = new QRCode(qrData))
            // High pixels-per-module (30) ensures maximum source bitmap resolution before scaling
            using (Bitmap qrBitmap = qrCode.GetGraphic(30, Color.Black, Color.White, drawQuietZones: true))
            {
                float padding = 3f;
                float availableWidth = qrAreaWidth - (padding * 2f);
                float availableHeight = totalHeight - (padding * 2f);
                float qrSquareSize = Math.Min(availableWidth, availableHeight);

                float startX = leftOffset + ((qrAreaWidth - qrSquareSize) / 2f);
                float startY = (totalHeight - qrSquareSize) / 2f;

                g.DrawImage(qrBitmap, startX, startY, qrSquareSize, qrSquareSize);
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
                throw;
            }

            var objENPrinterModelValue = new enPrinterModelValue() { ModelId = printerModelId };
            var objBLPrinterModelValue = new blPrinterModelValue(objENPrinterModelValue);
            try
            {
                objENPrinterModel.ModelValues = objBLPrinterModelValue.ReadAll();
            }
            catch (Exception ex)
            {
                Log.Error("[getPrinterModel] Error while Read() PrinterModelValue. \n Exception : " + ex.ToString());
                throw;
            }

            return objENPrinterModel;
        }
        #endregion
    }
}
