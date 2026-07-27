using System;
using System.IO.Ports;
using System.Web.Mvc;
using Entity;
using BusinessLogicLayer;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

namespace CodeScanner.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var genericDropdwon = new List<enGenericDropdown>();
            var supportedLanguagesList = new List<SelectListItem>();
            var listOfPorst = SerialPort.GetPortNames();
            var DistinctPorst = listOfPorst.Distinct().ToList();
            foreach (var item in DistinctPorst)
            {
                SelectListItem port = new SelectListItem { Value = item, Text = item, Selected = true };
                genericDropdwon.Add(new enGenericDropdown() { Key = item, Value = item });

                supportedLanguagesList.Add(port);
            }

            var printerDropdown = new List<enGenericDropdown>();
            PrintDocument pd = new PrintDocument();
            foreach (var item in PrinterSettings.InstalledPrinters)
            {
                printerDropdown.Add(new enGenericDropdown() { Key = item.ToString(), Value = item.ToString() });
            }

            var listOfOfficeMembers = new List<enOfficeMember>();
            var objENOfficeMember = new enOfficeMember();
            var objBLOfficeMember = new blOfficeMember(objENOfficeMember);
            try
            {
                listOfOfficeMembers = objBLOfficeMember.ReadAll();
            }
            catch (Exception ex)
            {
                throw;
            }

            ViewBag.VisualBy = listOfOfficeMembers.FindAll(x => x.Type == (int)Utility.OfficeMember.VisualBy);
            ViewBag.TestedBy = listOfOfficeMembers.FindAll(x => x.Type == (int)Utility.OfficeMember.TestedBy);
            ViewBag.ProcessEng = listOfOfficeMembers.FindAll(x => x.Type == (int)Utility.OfficeMember.ProcEng);
            ViewBag.ProgramDisplayNumber = listOfOfficeMembers.FindAll(x => x.Type == (int)Utility.OfficeMember.ProgDisNo);


            ViewBag.Ports = supportedLanguagesList.AsEnumerable<SelectListItem>().ToList();
            ViewBag.Printers = printerDropdown;
            ViewBag.gen = genericDropdwon;

            return View();
        }

        public ActionResult Setting()
        {
            List<enModel> listOfModels = new List<enModel>();
            var objENModel = new enModel();
            var objBLModel = new blModel(objENModel);
            try
            {
                listOfModels = objBLModel.ReadAll();
            }
            catch (Exception ex)
            {
                throw;
            }
            ViewBag.modelList = listOfModels;

            return View();
        }

        [HttpPost]
        public ActionResult Setting(enSetting Setting)
        {
            var objENSetting = new enSetting() { FileId = Setting.FileId };
            var objBLSetting = new blSetting(objENSetting);
            try
            {
                objBLSetting.Read();
            }
            catch (Exception ex)
            {
                return Json("f");
            }

            if (objENSetting.Id > 0)
            {
                try
                {
                    Setting.Id = objENSetting.Id;
                    Setting.CreatedOn = objENSetting.CreatedOn;
                    objBLSetting = new blSetting(Setting);
                    objBLSetting.Update();
                }
                catch (Exception ex)
                {
                    return Json("f");
                }


                try
                {
                    //settingInfo
                    var objENSettingInfo = new enSettingInfo() { SettingId = objENSetting.Id };
                    var objBLSettingInfo = new blSettingInfo(objENSettingInfo);
                    objBLSettingInfo.Delete();
                }
                catch (Exception ex)
                {
                    return Json("f");
                }
                var status = CreateSettingInfo(Setting.SettingInfo, objENSetting.Id);
            }
            else
            {
                try
                {
                    objBLSetting = new blSetting(Setting);
                    var id = objBLSetting.Create();
                    var status = CreateSettingInfo(Setting.SettingInfo, id);
                }
                catch (Exception ex)
                {
                    return Json("f");
                }

            }
            return Json("s");
        }

        public bool CreateSettingInfo(List<enSettingInfo> settingInfo, int settingId)
        {
            foreach (var item in settingInfo)
            {
                item.SettingId = settingId;
                var objBLSettingInfo = new blSettingInfo(item);
                try
                {
                    objBLSettingInfo.Create();
                }
                catch (Exception ex)
                {
                    return false;
                    throw;
                }

            }
            return true;
        }

        //public void PrintQrCode(string qrCode, string printerName)
        //{
        //    Log.Info("QrCode : " + qrCode + "PrinterName" + printerName);
        //    try
        //    {
        //        string folderPath = @"C:\Users\snu65\OneDrive\Desktop\utl_2026\qrCode";
        //        string fileNameWithExt = $"{qrCode.ToUpper()}.png";
        //        //string fullPath = Path.Combine(folderPath, fileNameWithExt);
        //        string fullPath = Path.Combine(folderPath, $"BGSX20NCY03KB000010_1.png");

        //        if (!System.IO.File.Exists(fullPath))
        //        {
        //            ViewBag.Error = "QR Code image file not found.";
        //            return;
        //        }

        //        if (string.IsNullOrWhiteSpace(printerName))
        //        {
        //            PrinterSettings settings = new PrinterSettings();
        //            printerName = settings.PrinterName;
        //        }

        //        PrintDocument pd = new PrintDocument();
        //        pd.DefaultPageSettings.PrinterSettings.PrinterName = printerName;
        //        pd.DefaultPageSettings.Landscape = false;

        //        PaperSize labelSize = new PaperSize("50x50mm", 197, 197);
        //        pd.DefaultPageSettings.PaperSize = labelSize;
        //        pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

        //        pd.PrintPage += (sender, args) =>
        //        {
        //            args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        //            args.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //            args.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        //            args.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        //            // -------------------------------------------------------------
        //            // 1. CHANGE THIS VALUE TO MOVE RIGHT:
        //            // Increase qrX (e.g., 50, 70, 100) to push it further right.
        //            // -------------------------------------------------------------
        //            // --- FINE-TUNED POSITION FOR 50x25mm LABEL ---
        //            float qrWidth = 75;  // ~19mm x 19mm QR code
        //            float qrHeight = 75;

        //            // 1. Shifted LEFT (Reduced from 59 to 38 to offset printer margin)
        //            float qrX = 30;

        //            // 2. Shifted DOWN (Increased from 2 to 10)
        //            float qrY = 8;

        //            RectangleF qrRectangle = new RectangleF(qrX, qrY, qrWidth, qrHeight);

        //            // Draw QR Code Image
        //            using (Image image = Image.FromFile(fullPath))
        //            {
        //                args.Graphics.DrawImage(image, qrRectangle);
        //            }

        //            // 3. Draw text right underneath QR code (moves along with the QR code)
        //            float textYPosition = qrY + qrHeight - 2;
        //            RectangleF textRectangle = new RectangleF(qrX - 20, textYPosition, qrWidth + 40, 18);

        //            using (Font textFont = new Font("Arial", 4, FontStyle.Regular))
        //            using (StringFormat format = new StringFormat())
        //            {
        //                format.Alignment = StringAlignment.Center;
        //                format.LineAlignment = StringAlignment.Near;
        //                format.FormatFlags = StringFormatFlags.NoClip;

        //                var staticqrName = "BGSX20NCY03KB000010_1";

        //                //string textToPrint = fileNameWithExt.ToUpper();
        //                string textToPrint = staticqrName.ToUpper();
        //                args.Graphics.DrawString(textToPrint, textFont, Brushes.Black, textRectangle, format);
        //            }
        //            args.HasMorePages = false;
        //        };

        //        pd.Print();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("/homecontroller/PrintQrCode error while print : " + ex.ToString());
        //        ViewBag.Error = "Failed to print...";
        //    }
        //}

        //Auto generate excel sheet
        public void GenerateExcel()
        {
            var objENResponse = new enResponse();
            var objBLResponse = new blResponse(objENResponse);
            var date = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            objENResponse.CurrentDate = date;
            List<enResponse> listOfResponses = new List<enResponse>();
            try
            {
                listOfResponses = objBLResponse.ReadAll();
            }
            catch (Exception ex)
            {
                throw;
            }


            var currentDay = DateTime.Now.DayOfWeek.ToString();
            var d = DateTime.Now.AddDays(-1);
            if (currentDay == "Monday")
            {
                d = DateTime.Now.AddDays(-2);
            }

            var objENDailyReport = new enDailyReport();
            objENDailyReport.Date = d.Day;
            objENDailyReport.Month = d.Month;
            objENDailyReport.Year = d.Year;
            var objBLDailyReport = new blDailyReport(objENDailyReport);
            try
            {
                objBLDailyReport.Read();
            }
            catch (Exception ex)
            {

            }

            if (objENDailyReport.Id == 0)
            {
                //var objENResponse = new enResponse();
                //var objBLResponse = new blResponse(objENResponse);
                //var date = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                //objENResponse.CurrentDate = date;
                //List<enResponse> listOfResponses = new List<enResponse>();
                //try
                //{
                //    listOfResponses = objBLResponse.ReadAll();
                //}
                //catch (Exception ex)
                //{
                //    throw;
                //}

                var listOfIds = listOfResponses.Select(x => x.Id).ToList();

                Log.Info("ListOfIds Count :" + listOfIds.Count);
                Log.Info("ListOfIds :" + listOfIds.ToString());
                var excelCnt = new ExcelController();
                var i = excelCnt.Download(listOfIds);

                if (i.Data == "s")
                {
                    try
                    {
                        objBLDailyReport.Create();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error While Create Daily Report but excel successfully created \n");
                        Log.Error("Exception : " + ex.ToString());
                    }
                }
                else
                {
                    Log.Error("Error to create excel on Daily Daily Report \n");
                    Log.Error("Exception : " + i.Data);
                }
            }
        }
    }
}