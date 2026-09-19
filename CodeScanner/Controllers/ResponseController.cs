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

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Json(new { success = false, message = "No ids provided" });

            int parsedId;
            var idList = ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .Where(x => int.TryParse(x, out parsedId))
                            .Select(int.Parse)
                            .ToList();

            if (!idList.Any())
                return Json(new { success = false, message = "No valid ids found" });

            var connStr = Utility.ApplicationSettings.DefaultConnectionString;
            int deletedCount = 0;

            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var parameters = idList.Select((id, index) => $"@id{index}").ToArray();
                        string inClause = string.Join(",", parameters);

                        // 1. Delete child records first (Update 'ResponseSummary' to match your SQL DB table name)
                        string deleteSummarySql = $"DELETE FROM ResponseSummary WHERE ResponseId IN ({inClause})";
                        using (var cmdSummary = new System.Data.SqlClient.SqlCommand(deleteSummarySql, conn, tx))
                        {
                            for (int i = 0; i < idList.Count; i++)
                            {
                                cmdSummary.Parameters.AddWithValue($"@id{i}", idList[i]);
                            }
                            cmdSummary.ExecuteNonQuery();
                        }

                        // 2. Delete parent records (Update 'Response' to match your SQL DB table name)
                        string deleteResponseSql = $"DELETE FROM Response WHERE Id IN ({inClause})";
                        using (var cmdResponse = new System.Data.SqlClient.SqlCommand(deleteResponseSql, conn, tx))
                        {
                            for (int i = 0; i < idList.Count; i++)
                            {
                                cmdResponse.Parameters.AddWithValue($"@id{i}", idList[i]);
                            }
                            deletedCount = cmdResponse.ExecuteNonQuery();
                        }

                        tx.Commit();
                        Log.Info($"DeleteAll: Bulk delete committed. Total records deleted: {deletedCount}");
                        return Json(new { success = true, deleted = deletedCount });
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        Log.Error("DeleteAll: Transaction rolled back due to error: " + ex);
                        return Json(new { success = false, message = ex.Message });
                    }
                }
            }
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
            //string sortColumns = Request.Form["order[0][column]"];
            ////Finding the column name from the list based upon the column Index  
            //int sortColumn = sortColumns[Convert.ToInt32(sortColumns)];
            //Sorting Direction  
            //string sortDirection = Request.Form["order[0][dir]"];

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
                try { Log.Error("GetResponse: Failed to read responses: " + ex); } catch { }
                // Return an empty result set to the client instead of throwing an exception that breaks the page
                listOfResponse = new List<enResponse>();
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
                enResponsetblResp.Status = item.ResponseSummary != null ? item.ResponseSummary.Status == "PASS" ? "green" : "red" : "blank";
                responseList.Add(enResponsetblResp);
            }
            // Ensure DataTables receives the correct totals. The DB returns TotalRecords (total matching rows)
            // in each row's RecordsCount field; use that as the overall total. Set both recordsTotal and
            // recordsFiltered to that value so the table shows the correct total count.
            int totalMatchingRecords = listOfResponse.Count() == 0 ? 0 : listOfResponse.FirstOrDefault().RecordsCount;
            var response = new enDataTableResp()
            {
                draw = ajaxDraw,
                recordsTotal = totalMatchingRecords,
                recordsFiltered = totalMatchingRecords,
                data = responseList
            };
            #endregion

            return Json(response);
        }
    }
}