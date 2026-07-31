using BusinessLogicLayer;
using Entity;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeScanner.Controllers
{
    public class SettingController : BaseController
    {
        // GET: Setting
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetByFileId(string fileId)
        {
            var objENSetting = new enSetting() { FileId = fileId };
            var objBLSetting = new blSetting(objENSetting);
            try
            {
                objBLSetting.ReadAndAggregate(typeof(enSettingInfo));
            }
            catch (Exception ex)
            {
                throw;
            }

            return Json(objENSetting, JsonRequestBehavior.AllowGet);
        }
    }
}