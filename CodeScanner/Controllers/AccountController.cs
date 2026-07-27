using System;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Entity;
using IronBarCode;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;

namespace CodeScanner.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(enAdmin enAdmin_)
        {
            if (enAdmin_.UserID == "utlups" && enAdmin_.Password == "UTL@123456")
            {
                HttpCookie loginCookie = new HttpCookie("LoginCookie");
                var cookieDetail = new enCookieDetail { UserName = "Admin" };
                loginCookie.Value = JsonConvert.SerializeObject(cookieDetail);
                loginCookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(loginCookie);
                return RedirectToAction("index", "home");
            }
            ViewBag.Error = "Password Not Match";
            return View();
        }

        public ActionResult Logout()
        {
            if (Request.Cookies["LoginCookie"] != null)
            {
                Response.Cookies["LoginCookie"].Expires = DateTime.Now.AddDays(-1);
            }
            return RedirectToAction("Login");
        }

        public ActionResult Testing()
        {
            var QrCodePath = Utility.ApplicationSettings.getQrCodePath;
            var QrCodeString = "BarCode :  visualBy :  testedBy :  productionLine :  lineIncharge : currentDate :  time :  Display Program No. :  Control Program No. ";

            QRCodeWriter.CreateQrCode(QrCodeString, 250, QRCodeWriter.QrErrorCorrectionLevel.Medium).ChangeBarCodeColor(Color.Red).SaveAsPng(QrCodePath + "\\qrCode123.png");
            //QRCodeWriter.CreateQrCodeWithLogoImage(QrCodeString, img, 500, 0).ChangeBarCodeColor(Color.Red).SaveAsPng("D:\\QrCodeQrCode123.svg");

            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.PrinterSettings.PrinterName = "TSC TA210";
            //pd.DefaultPageSettings.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            pd.DefaultPageSettings.Landscape = false; //or false!
            pd.PrintPage += (sender, args) =>
            {
                var path = Utility.ApplicationSettings.getQrCodePath;
                Image image = Image.FromFile(path + @"\\QrCode123.png");

                RectangleF rectf = new RectangleF(50, 90, 125, 125);
                Graphics g = Graphics.FromImage(image);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.FillRectangle(Brushes.White,50,90,130,50);
                g.DrawString("GPPXCDU01KA023938", new Font("Arial", 16), Brushes.Black,rectf);
                g.Flush();
                args.Graphics.DrawImage(image, rectf);
            };
            pd.Print();
            return null;
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

        }
    }
}