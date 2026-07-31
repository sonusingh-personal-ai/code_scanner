using BusinessLogicLayer;
using Entity;
using Entity.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using System.IO.Ports;

namespace CodeScanner.Controllers
{
    public class RequestQrCodeController : Controller
    {
        static SerialPort _serialPort;

        // GET: RequestQrCode
        //public JsonResult Request(enQrCodeResponse qrCodeResponse)
        //{
        //    enSettingResponse matchString = new enSettingResponse();
        //    var b = "#" + qrCodeResponse.BarCodeString + "@";
        //    var barcodeLength = qrCodeResponse.BarCodeString.Length;
        //    var str = "";

        //    var QrCodePath = ApplicationSettings.getQrCodePath;
        //    if (!Directory.Exists(QrCodePath))
        //    {
        //        matchString.status = (int)ResponseStatus.Fail;
        //        matchString.message = "Directory not exist. \n Path : " + QrCodePath;
        //        return Json(matchString, JsonRequestBehavior.AllowGet);
        //    }

        //    if (barcodeLength == 19 || barcodeLength == 17)
        //    {
        //        string IsThree = qrCodeResponse.BarCodeString.Substring(0, 1);
        //        if (IsThree == "3")
        //        {
        //            str = qrCodeResponse.BarCodeString.Substring(3, 3);
        //        }
        //        else
        //        {
        //            str = qrCodeResponse.BarCodeString.Substring(0, 3);
        //        }
        //    }

        //    if (str == "")
        //    {
        //        matchString.status = (int)ResponseStatus.Fail;
        //        matchString.message = "Model does not exist.";
        //        return Json(matchString, JsonRequestBehavior.AllowGet);
        //    }

        //    var model = IsModelExists(str);
        //    if (model.Id == 0)
        //    {
        //        matchString.status = (int)ResponseStatus.Fail;
        //        matchString.message = "Model does not exist.";
        //        return Json(matchString, JsonRequestBehavior.AllowGet);
        //    }

        //    var objENResponse = new Entity.enResponse() { Barcode = qrCodeResponse.BarCodeString };
        //    var objBLResponse = new blResponse(objENResponse);
        //    try
        //    {
        //        objBLResponse.Read();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("COMPostController.SendParameter Error while read Response()");
        //    }

        //    if (qrCodeResponse.IsRepeat)
        //    {
        //        try
        //        {
        //            objBLResponse.Delete();
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error("COMPostController.SendParameter Error while Delete Response() /n Exp: " + ex);
        //        }

        //        var objENResponseSummary = new enResponseSummary() { ResponseId = objENResponse.Id };
        //        var objBLResponseSummary = new blResponseSummary(objENResponseSummary);
        //        try
        //        {
        //            objBLResponseSummary.Delete();
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error("COMPostController.SendParameter Error while Delete ResponseSummary() /n Exp: " + ex);
        //        }
        //    }

        //    var setting = getSettingFile(str);
        //    if (setting.Id == 0)
        //    {
        //        matchString.status = (int)ResponseStatus.Fail;
        //        matchString.message = "Setting file is missing";
        //        return Json(matchString, JsonRequestBehavior.AllowGet);
        //    }

        //    string[] response = new string[] { };
        //    try
        //    {
        //        if (isRecurance == false)
        //        {
        //            _serialPort = new SerialPort();
        //            _serialPort.PortName = qrCodeResponse.PortNumber;
        //            _serialPort.BaudRate = qrCodeResponse.BaudRate;
        //            _serialPort.Parity = SetPortParity(_serialPort.Parity);
        //            _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
        //            _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
        //            _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);
        //            _serialPort.Close();
        //            _serialPort.Dispose();
        //            _serialPort.Open();

        //            _serialPort.WriteLine(b);
        //            Log.Info("Barcode : " + b);
        //        }

        //        List<List<string>> stringObject = new List<List<string>>();
        //        var rt = _serialPort.ReadTimeout;
        //        DateTime now = DateTime.Now;
        //        var t = DateTime.Now.Subtract(now).Seconds;
        //        var count = 1;
        //        #region while loop

        //        while (t < 120)
        //        {

        //            var rec = _serialPort.ReadLine();
        //            Log.Info("Receving string");
        //            Log.Info(rec);
        //            if (rec.Length != 4)
        //            {
        //                var isExist = rec.LastIndexOf("@");
        //                var CarretIndx = rec.LastIndexOf("^");

        //                Log.Info("@ index : " + isExist);
        //                Log.Info("^ index : " + CarretIndx);

        //                if (isExist > -1 && CarretIndx > -1)
        //                {
        //                    var nrec = rec.Substring(isExist, (CarretIndx + 1) - isExist);
        //                    Log.Info(nrec);
        //                    response = nrec.Split(',');
        //                    var ConProgNo = response[1];
        //                    var DisProgNo = response[2];
        //                    if (response.Length > 3)
        //                    {
        //                        Log.Info("Length");
        //                        Log.Info(response.Length.ToString());
        //                        var matchStr = response[response.Length - 2];
        //                        var unResponsive = response[response.Length - 3];

        //                        Log.Info("****UnResponsive***");
        //                        Log.Info(unResponsive);
        //                        if (unResponsive == "SCAN CODE")
        //                        {
        //                            _serialPort.Close();
        //                            _serialPort.Dispose();
        //                            return Json(unResponsive, JsonRequestBehavior.AllowGet);
        //                        }

        //                        Log.Info(matchStr);
        //                        if (matchStr == "FAIL" || matchStr == "PASS")
        //                        {
        //                            Log.Info("****** Final Result ******");
        //                            Log.Info("****** " + matchStr + " ******");
        //                            Log.Error("**** Process : " + count + "  || END ");


        //                            t = 121;
        //                            if (matchStr == "FAIL")
        //                            {
        //                                Log.Info("****** Result FAIL ******");
        //                                var resp = SaveReponse(setting, response, barcode, true, visualby, testedBy, productionLine, lineInCharge, testingJig, currentDate, Time, true, ConProgNo, DisProgNo);
        //                                matchString = CompairFile(setting, response);
        //                                matchString.totalString = stringObject;
        //                                matchString.SettingInfoList = setting.SettingInfo;
        //                                matchString.model = setting.Model.Name;
        //                                matchString.isOk = true;
        //                                //var img =ConvertStringToImage(barcode);
        //                                var QrCodeString = "BarCode : " + barcode + "\n visualBy : " + visualby + "\n testedBy : " + testedBy + "\n productionLine : " + productionLine + "\n lineIncharge : " + lineInCharge + "\n currentDate : " + currentDate + "\n time : " + Time + "\n Display Program No. : " + matchString.displayPv + "\n Control Program No. : " + matchString.controlPv;
        //                                //QRCodeWriter.CreateQrCodeWithLogoImage(QrCodeString, img, 500, 0).ChangeBarCodeColor(Color.Red).SaveAsPng(QrCodePath + "\\" + barcode + ".png");
        //                                QRCodeWriter.CreateQrCode(QrCodeString, 250, QRCodeWriter.QrErrorCorrectionLevel.Medium).ChangeBarCodeColor(Color.Red).SaveAsPng(QrCodePath + "\\" + barcode + ".png");

        //                                _serialPort.Close();
        //                                _serialPort.Dispose();
        //                                return Json(matchString, JsonRequestBehavior.AllowGet);
        //                            }
        //                            else
        //                            {
        //                                Log.Info("****** Result Pass ******");
        //                                var resp = SaveReponse(setting, response, barcode, true, visualby, testedBy, productionLine, lineInCharge, testingJig, currentDate, Time, false, ConProgNo, DisProgNo);
        //                                matchString = CompairFile(setting, response);
        //                                matchString.totalString = stringObject;
        //                                matchString.SettingInfoList = setting.SettingInfo;
        //                                matchString.model = setting.Model.Name;
        //                                matchString.isOk = true;
        //                                //var img = ConvertStringToImage(barcode);
        //                                var QrCodeString = "BarCode : " + barcode + "\n visualBy : " + visualby + "\n testedBy : " + testedBy + "\n productionLine : " + productionLine + "\n lineIncharge : " + lineInCharge + "\n currentDate : " + currentDate + "\n time : " + Time + "\n Display Program No. : " + matchString.displayPv + "\n Control Program No. : " + matchString.controlPv;
        //                                //QRCodeWriter.CreateQrCodeWithLogoImage(QrCodeString, img, 500, 0).ChangeBarCodeColor(Color.SkyBlue).SaveAsPng(QrCodePath + "\\" + barcode + ".png");
        //                                QRCodeWriter.CreateQrCode(QrCodeString, 250, QRCodeWriter.QrErrorCorrectionLevel.Medium).ChangeBarCodeColor(Color.SkyBlue).SaveAsPng(QrCodePath + "\\" + barcode + ".png");
        //                                _serialPort.Close();
        //                                _serialPort.Dispose();
        //                                return Json(matchString, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            Log.Error("**** Process Start : " + count + "  || request ");
        //                            count++;
        //                            stringObject.Add(response.ToList());
        //                            var resp = SaveReponse(setting, response, barcode, false, visualby, testedBy, productionLine, lineInCharge, testingJig, currentDate, Time, false, ConProgNo, DisProgNo);
        //                            matchString = CompairFile(setting, response);
        //                            matchString.totalString = stringObject;
        //                            matchString.SettingInfoList = setting.SettingInfo;
        //                            matchString.model = setting.Model.Name;
        //                            return Json(matchString, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //        #endregion

        //    }
        //    catch (Exception ex)
        //    {
        //        _serialPort.Close();
        //        _serialPort.Dispose();
        //        matchString.status = (int)ResponseStatus.Fail;
        //        matchString.message = ex.ToString();
        //        matchString.isOk = true;
        //        Log.Error("Exception : " + ex.ToString());
        //        return Json(matchString, JsonRequestBehavior.AllowGet);
        //    }
        //    _serialPort.Close();
        //    _serialPort.Dispose();

        //    return Json(matchString, JsonRequestBehavior.AllowGet);
        //}
    }
}