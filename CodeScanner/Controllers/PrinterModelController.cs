using Entity;
using System.Web.Mvc;
using BusinessLogicLayer;
using System;
using System.Collections.Generic;

namespace CodeScanner.Controllers
{
    public class PrinterModelController : BaseController
    {
        // GET: PrinterModel
        public ActionResult Index()
        {
            LoadViewData();
            return View();
        }

        #region PrinterModel Actions

        [HttpPost]
        public ActionResult CreatePrinterModel(enPrinterModel model)
        {
            var objBLPrinterModel = new blPrinterModel(model);
            try
            {
                objBLPrinterModel.Create();
            }
            catch (Exception ex)
            {
                throw;
            }

            return RedirectToAction("Index");
        }

        public ActionResult UpdatePrinterModel(int? id)
        {
            if (id == null || id.Value < 1)
            {
                return RedirectToAction("Error", "Misc");
            }

            var objENPrinterModel = new enPrinterModel { Id = id.Value };
            var objBLPrinterModel = new blPrinterModel(objENPrinterModel);
            try
            {
                objBLPrinterModel.Read();
            }
            catch
            {
            }

            LoadViewData();
            return View("Index", objENPrinterModel);
        }

        [HttpPost]
        public ActionResult UpdatePrinterModel(int? id, enPrinterModel model)
        {
            if (id.HasValue && id.Value > 0)
            {
                var objENPrinterModel = new enPrinterModel() { Id = id.Value };
                var objBLPrinterModel = new blPrinterModel(objENPrinterModel);
                try
                {
                    objBLPrinterModel.Read();
                }
                catch (Exception ex)
                {
                    throw;
                }

                if (objENPrinterModel.Id > 0)
                {
                    model.CreatedOn = objENPrinterModel.CreatedOn;
                    objBLPrinterModel = new blPrinterModel(model);
                    try
                    {
                        objBLPrinterModel.Update();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult DeletePrinterModel(int id)
        {
            try
            {
                var objENPrinterModel = new enPrinterModel { Id = id };
                var objBLPrinterModel = new blPrinterModel(objENPrinterModel);
                int result = objBLPrinterModel.Delete();

                return Json(result > 0 ? "s" : "f");
            }
            catch
            {
                return Json("f");
            }
        }

        #endregion

        #region PrinterModelValue Actions

        [HttpPost]
        public ActionResult CreatePrinterModelValue(enPrinterModelValue model)
        {
            var objBLPrinterModelValue = new blPrinterModelValue(model);
            try
            {
                objBLPrinterModelValue.Create();
            }
            catch (Exception ex)
            {
                throw;
            }

            return RedirectToAction("Index");
        }

        public ActionResult UpdatePrinterModelValue(int? id)
        {
            if (id == null || id.Value < 1)
            {
                return RedirectToAction("Error", "Misc");
            }

            var objENPrinterModelValue = new enPrinterModelValue { Id = id.Value };
            var objBLPrinterModelValue = new blPrinterModelValue(objENPrinterModelValue);
            try
            {
                objBLPrinterModelValue.Read();
            }
            catch
            {
            }

            LoadViewData();
            // Store the value item in ViewBag or pass an object the view accepts
            ViewBag.SelectedPrinterModelValue = objENPrinterModelValue;
            return View("Index");
        }

        [HttpPost]
        public ActionResult UpdatePrinterModelValue(int? id, enPrinterModelValue model)
        {
            if (id.HasValue && id.Value > 0)
            {
                var objENPrinterModelValue = new enPrinterModelValue() { Id = id.Value };
                var objBLPrinterModelValue = new blPrinterModelValue(objENPrinterModelValue);
                try
                {
                    objBLPrinterModelValue.Read();
                }
                catch (Exception ex)
                {
                    throw;
                }

                if (objENPrinterModelValue.Id > 0)
                {
                    model.CreatedOn = objENPrinterModelValue.CreatedOn;
                    objBLPrinterModelValue = new blPrinterModelValue(model);
                    try
                    {
                        objBLPrinterModelValue.Update();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult DeletePrinterModelValue(int id)
        {
            try
            {
                var objENPrinterModelValue = new enPrinterModelValue { Id = id };
                var objBLPrinterModelValue = new blPrinterModelValue(objENPrinterModelValue);
                int result = objBLPrinterModelValue.Delete();

                return Json(result > 0 ? "s" : "f");
            }
            catch
            {
                return Json("f");
            }
        }

        #endregion

        private void LoadViewData()
        {
            var listOfPrinterModels = new List<enPrinterModel>();
            var listOfPrinterModelValues = new List<enPrinterModelValue>();

            try
            {
                var objBLPrinterModel = new blPrinterModel(new enPrinterModel());
                listOfPrinterModels = objBLPrinterModel.ReadAll();

                var objBLPrinterModelValue = new blPrinterModelValue(new enPrinterModelValue());
                listOfPrinterModelValues = objBLPrinterModelValue.ReadAll();
            }
            catch (Exception ex)
            {
                throw;
            }

            ViewBag.PrinterModelList = listOfPrinterModels ?? new List<enPrinterModel>();
            ViewBag.PrinterModelValueList = listOfPrinterModelValues ?? new List<enPrinterModelValue>();
        }
    }
}