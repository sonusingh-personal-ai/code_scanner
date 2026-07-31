using BusinessLogicLayer;
using Entity;
using Entity.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CodeScanner.Controllers
{
    public class ResponseController : BaseController
    {
        // GET: Response
        //public ActionResult Index()
        //{
        //    List<enResponse> listOfResponse = new List<enResponse>();
        //    var objENResponse = new enResponse();
        //    var objBLResponse = new blResponse(objENResponse);
        //    try
        //    {
        //        listOfResponse = objBLResponse.ReadAllAndAggregate(null,null,null,null,typeof(enResponseSummary));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    return View(listOfResponse);
        //}

        public ActionResult GetResponseSummary(int id)
        {
            List<enResponseSummary> listOfResponseSummary = new List<enResponseSummary>();
            var objENResponseSummary = new enResponseSummary() { ResponseId = id };
            var objBLResponseSummary = new blResponseSummary(objENResponseSummary);
            try
            {
                listOfResponseSummary = objBLResponseSummary.ReadAll().FindAll(x => x.IsFinal == true);
            }
            catch (Exception ex)
            {
                throw;
            }

            return View(listOfResponseSummary);
        }

        public ActionResult DeleteResponseSummary(int id)
        {
            var objENResponse = new enResponse() { Id = id };
            var objBLResponse = new blResponse(objENResponse);
            try
            {
                objBLResponse.Delete();
            }
            catch (Exception ex)
            {
                Log.Error("Error while Delete Response where id is :" + id + " \n Error : " + ex);
            }

            var objENResponseSummary = new enResponseSummary() { ResponseId = id };
            var objBLResponseSummary = new blResponseSummary(objENResponseSummary);
            try
            {
                objBLResponseSummary.Delete();
            }
            catch (Exception ex)
            {
                Log.Error("Error while Delete Response summary where id is :" + id + " \n Error : " + ex);
            }
            return RedirectToAction("index");
        }

        public ActionResult DeleteAll(string ids)
        {
            var idArray = ids.Split(',');
            foreach (var item in idArray)
            {
                var id = Convert.ToInt32(item);
                var objENResponse = new enResponse() { Id = id };
                var objBLResponse = new blResponse(objENResponse);
                try
                {
                    objBLResponse.Delete();
                }
                catch (Exception ex)
                {
                    Log.Error("Error while Delete Response where id is :" + id + " \n Error : " + ex);
                }

                var objENResponseSummary = new enResponseSummary() { ResponseId = id };
                var objBLResponseSummary = new blResponseSummary(objENResponseSummary);
                try
                {
                    objBLResponseSummary.Delete();
                }
                catch (Exception ex)
                {
                    Log.Error("Error while Delete Response summary where id is :" + id + " \n Error : " + ex);
                }
            }

            return Json("s", JsonRequestBehavior.AllowGet);
        }

        public ActionResult index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetResponse()
        {
            Int32 ajaxDraw = Convert.ToInt32(Request.Form["draw"]);
            //OffsetValue  
            Int32 OffsetValue = Convert.ToInt32(Request.Form["start"]);
            //No of Records shown per page  
            Int32 PagingSize = Convert.ToInt32(Request.Form["length"]);
            //Getting value from the seatch TextBox  
            string searchby = Request.Form["search[value]"];
            //Index of the Column on which Sorting needs to perform  
            string sortColumns = Request.Form["order[0][column]"];
            //Finding the column name from the list based upon the column Index  
            int sortColumn = sortColumns[Convert.ToInt32(sortColumns)];
            //Sorting Direction  
            string sortDirection = Request.Form["order[0][dir]"];

            string startDate = Request.Form["startDate"];
            string endDate = Request.Form["endDate"];

            #region response from database
            List<enResponse> listOfResponse = new List<enResponse>();
            var objENResponse = new enResponse();
            var objBLResponse = new blResponse(objENResponse);
            try
            {
                int _startRowNumber = OffsetValue + 1;
                int _endRowNumber = PagingSize + OffsetValue;

                if (searchby != "" && startDate != "" && endDate != "")
                {
                    DateTime sDate = DateTime.ParseExact(startDate, "dd/MM/yyyy", null);
                    DateTime eDate = DateTime.ParseExact(endDate, "dd/MM/yyyy", null);
                    listOfResponse = objBLResponse.ReadAllAndAggregate(_startRowNumber, _endRowNumber, sDate, eDate.AddDays(1), searchby.ToUpper(), typeof(enResponseSummary));
                    // Apply client-side filtering correctly (previous code did not assign the filtered result)
                    listOfResponse = listOfResponse.Where(x =>
                        (x.Barcode != null && x.Barcode.IndexOf(searchby, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (x.Model != null && x.Model.IndexOf(searchby, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (x.SystemRating != null && x.SystemRating.IndexOf(searchby, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (x.SerialCardNo != null && x.SerialCardNo.IndexOf(searchby, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                }
                else if (searchby != "")
                {
                    listOfResponse = objBLResponse.ReadAllAndAggregate(_startRowNumber, _endRowNumber, null, null, searchby.ToUpper(), typeof(enResponseSummary));
                }
                else if (startDate != "" && endDate != "")
                {
                    DateTime sDate = DateTime.ParseExact(startDate, "dd/MM/yyyy", null);
                    DateTime eDate = DateTime.ParseExact(endDate, "dd/MM/yyyy", null);
                    listOfResponse = objBLResponse.ReadAllAndAggregate(_startRowNumber, _endRowNumber, sDate, eDate.AddDays(1), null, typeof(enResponseSummary));//();
                }
                else
                {
                    listOfResponse = objBLResponse.ReadAllAndAggregate(_startRowNumber, _endRowNumber, null, null, null, typeof(enResponseSummary));// ReadAll(_startRowNumber, _endRowNumber);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            #endregion

            #region attach response list
            List<enOfficeMember> listOfOfficeMember = new List<enOfficeMember>();
            var objENOfficeMember = new enOfficeMember();
            var objBLOfficeMember = new blOfficeMember(objENOfficeMember);
            try
            {
                listOfOfficeMember = objBLOfficeMember.ReadAll();
            }
            catch (Exception)
            {
                throw;
            }

            List<enResponseTblResp> responseList = new List<enResponseTblResp>();
            // build a lookup for office members to avoid repeated list scans
            var officeLookup = listOfOfficeMember.ToDictionary(x => x.ID, x => x.Name);
            foreach (var item in listOfResponse)
            {
                var enResponsetblResp = new enResponseTblResp();
                enResponsetblResp.Id = item.Id;
                enResponsetblResp.Sno = item.RowNumber;
                enResponsetblResp.BarCode = item.Barcode;
                enResponsetblResp.Model = item.Model;
                enResponsetblResp.SysRating = item.SystemRating;
                string name;
                enResponsetblResp.VisualBy = officeLookup.TryGetValue(item.VisualBy, out name) ? name : string.Empty;
                enResponsetblResp.ProdLine = Utility.Helper.ProductionLine(item.ProductionLine);
                enResponsetblResp.TestedBy = officeLookup.TryGetValue(item.TestedBy, out name) ? name : string.Empty;
                enResponsetblResp.ProcEng = officeLookup.TryGetValue(item.ProcessEngg, out name) ? name : string.Empty;
                enResponsetblResp.QcStatus = Utility.Helper.TestingStage(item.QcStatus);
                enResponsetblResp.CardSerNo = item.SerialCardNo;
                enResponsetblResp.Date = item.CreatedOn.ToString();
                //enResponsetblResp.Status = item.listOfResponseSummary.Count == 0 ? "" : item.listOfResponseSummary != null ? (item.listOfResponseSummary.Last().Status == "PASS" ? "green" : "red") : "";
                enResponsetblResp.Status = item.ResponseSummary.Status == "PASS" ? "green" : "red";
                responseList.Add(enResponsetblResp);
            }
            var response = new enDataTableResp()
            {
                draw = ajaxDraw,
                recordsFiltered = listOfResponse.Count() == 0 ? 0 : listOfResponse.FirstOrDefault().RecordsCount,
                recordsTotal = listOfResponse.Count(),
                data = responseList
            };
            #endregion

            return Json(response);
        }
    }
}