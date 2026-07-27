using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLogicLayer;

namespace CodeScanner.Controllers
{
    public class OfficeMemberController : Controller
    {
        // GET: OfficeMember
        public ActionResult Index()
        {
            List<enOfficeMember> listOfOfficeMemebers = new List<enOfficeMember>();
            var objENOfficeMember = new enOfficeMember();
            var objBLOfficeMember = new blOfficeMember(objENOfficeMember);
            try
            {
                listOfOfficeMemebers = objBLOfficeMember.ReadAll();
            }
            catch (Exception ex)
            {
                throw;
            }
            ViewBag.OfficeMemberList = listOfOfficeMemebers;
            return View();
        }

        [HttpPost]
        public ActionResult Index(enOfficeMember officeMember)
        {
            var objBLOfficeMember = new blOfficeMember(officeMember);
            try
            {
                objBLOfficeMember.Create();
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
            var objENOfficeMember = new enOfficeMember { ID = id.Value };
            var objBLOfficeMember = new blOfficeMember(objENOfficeMember);
            try
            {
                objBLOfficeMember.Read();
            }
            catch
            {
                throw;
            }

            return View("index", objENOfficeMember);
        }

        [HttpPost]
        public ActionResult Update(int? id, enOfficeMember officeMember)
        {
            var objENOfficeMember = new enOfficeMember() { ID = id.Value };
            var objBLOfficeMember = new blOfficeMember(objENOfficeMember);
            try
            {
                objBLOfficeMember.Read();
            }
            catch (Exception ex)
            {
                throw;
            }

            if (objENOfficeMember.ID > 0)
            {
                officeMember.InsertedOn = objENOfficeMember.InsertedOn;
                objBLOfficeMember = new blOfficeMember(officeMember);
                try
                {
                    objBLOfficeMember.Update();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return RedirectToAction("index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null || id.Value < 1)
            {
                return RedirectToAction("error", "misc");
            }
            var objENOfficeMember = new enOfficeMember { ID = id.Value };
            var objBLOfficeMember = new blOfficeMember(objENOfficeMember);
            try
            {
                objBLOfficeMember.Delete();
            }
            catch
            {
                return RedirectToAction("error", "misc");
            }

            return RedirectToAction("index");
        }


    }
}