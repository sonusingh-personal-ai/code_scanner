using BusinessLogicLayer;
using Entity;
using Entity.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;

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

        public static List<enResponseSummary> SaveReponse(enSetting setting, string[] response, string barcode, bool isOk, int qcStatus, int visualby, int testedBy, int productionLine, int processEngg, string testingJig, string currentDate, string Time, bool barCodeStatus, string ConProgNo, string DisProgNo,string SysRating)
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
                objENResponse.Model = setting.Model.Name;
                objENResponse.ConProgNo = ConProgNo;
                objENResponse.DisProgNo = DisProgNo;
                objENResponse.SystemRating = SysRating;
                objENResponse.CurrentDate = currentDate;
                objENResponse.CurrentTime = Time;
                objENResponse.ResponseTime = DateTime.Now;

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
    }
}