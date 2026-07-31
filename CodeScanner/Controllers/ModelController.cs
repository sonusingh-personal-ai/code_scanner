using Entity;
using System.Web.Mvc;
using BusinessLogicLayer;
using System;
using System.Collections.Generic;

namespace CodeScanner.Controllers
{
    public class ModelController : BaseController
    {
        // GET: Model
        public ActionResult Index()
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

            ViewBag.ModelList = listOfModels;
            ViewBag.QrCodePath = Utility.ApplicationSettings.getQrCodePath;
            ViewBag.Excelpath = Utility.ApplicationSettings.getExcelPath;
            return View();
        }

        [HttpPost]
        public ActionResult Index(enModel model)
        {
            var objBLModel = new blModel(model);
            try
            {
                objBLModel.Create();
            }
            catch (Exception ex)
            {
                throw;
            }

            return RedirectToAction("index");
        }

        public ActionResult Update(int? id)
        {
            if (id == null || id.Value < 1)
            {
                return RedirectToAction("error", "misc");
            }
            var objENModel = new enModel { Id = id.Value };
            var objBLModel = new blModel(objENModel);
            try
            {
                objBLModel.Read();
            }
            catch
            {
            }

            return View("index", objENModel);
        }

        [HttpPost]
        public ActionResult Update(int? id, enModel model)
        {
            var objENModel = new enModel() { Id = id.Value };
            var objBLModel = new blModel(objENModel);
            try
            {
                objBLModel.Read();
            }
            catch (Exception ex)
            {
                throw;
            }

            if (objENModel.Id > 0)
            {
                model.CreatedOn = objENModel.CreatedOn;
                objBLModel = new blModel(model);
                try
                {
                    objBLModel.Update();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return RedirectToAction("index");
        }

        public ActionResult List()
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
            return View(listOfModels);
        }

        public JsonResult UpdatePath(string qrCodePath, string excelPath)
        {
            //var qrCode = "";
            //var excPath = "";

            //if (qrCodePath != "")
            //    qrCode = new System.Uri(qrCodePath).AbsoluteUri.Remove(0, 8);
            //if (excelPath != "")
            //    excPath = new System.Uri(excelPath).AbsoluteUri.Remove(0, 8);

            try
            {
                Utility.ApplicationSettings.UpdatePath(qrCodePath, excelPath);
            }
            catch (Exception ex)
            {
                return Json("f", JsonRequestBehavior.AllowGet);
            }
            return Json("s", JsonRequestBehavior.AllowGet);
        }
    }
}