using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Entity;
using BusinessLogicLayer;
using System.Linq;

namespace CodeScanner.Controllers
{
    public class ExcelController : Controller
    {
        [HttpPost]
        public JsonResult Download(List<int> ids)
        {
            var excelPath = Utility.ApplicationSettings.getExcelPath;
            if (!Directory.Exists(excelPath))
            {
                return Json(" File Path Not Exist ", JsonRequestBehavior.AllowGet);
            }

            var objENResponse = new enResponse();
            var objBLResponse = new blResponse(objENResponse);
            List<enResponse> listOfResponses = new List<enResponse>();

            try
            {
                listOfResponses = objBLResponse.ReadAll().Where(x => ids.Contains(x.Id)).ToList();

                foreach(var response in listOfResponses)
                {
                    var objBLResponseSummary = new blResponseSummary(new enResponseSummary() { ResponseId = response.Id});
                    response.listOfResponseSummary = objBLResponseSummary.ReadAll() ;
                }

                //listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, null, null, null, typeof(enResponseSummary), typeof(enOfficeMember)).Where(x => ids.Contains(x.Id)).ToList();
            }
            catch (Exception ex)
            {
                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }

            if (listOfResponses.Count > 0)
            {

                var listOfOfficeMemeber = new List<enOfficeMember>();
                var objBLOfficeMember = new blOfficeMember(new enOfficeMember());
                listOfOfficeMemeber = objBLOfficeMember.ReadAll();

                try
                {
                    //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    ExcelPackage excel = new ExcelPackage();

                    var workSheet = excel.Workbook.Worksheets.Add(listOfResponses[0].Model);
                    workSheet.TabColor = System.Drawing.Color.Black;
                    workSheet.DefaultRowHeight = 12;

                    workSheet.Row(1).Height = 20;
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(1).Style.Font.Bold = true;

                    workSheet.Cells[1, 1].Value = "Testing Zig";
                    var date = listOfResponses[0].CurrentDate;

                    var i = 1;
                    var j = 2;
                    var k = 4;
                    foreach (var item in listOfResponses)
                    {
                        if (i > 1)
                        {
                            j = j + 3;
                            k = k + 3;
                        }

                        workSheet.Cells[3, 1].Value = "DATE & TIME";
                        workSheet.Cells[3, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[3, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[3, j, 3, k].Merge = true;
                        workSheet.Cells[3, j].Value = item.CreatedOn.ToString();

                        workSheet.Cells[4, 1].Value = "VISUAL";
                        workSheet.Cells[4, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[4, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[4, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[4, j, 4, k].Merge = true;



                        //var objENOfficeMember = new enOfficeMember() { ID = item.VisualBy };
                        //var objBLOfficeMember = new blOfficeMember(objENOfficeMember);
                        //objBLOfficeMember.Read();

                        var obj = listOfOfficeMemeber.Find(x => x.ID == item.VisualBy);
                        workSheet.Cells[4, j].Value = obj == null ? "" : obj.Name;

                        workSheet.Cells[5, 1].Value = "PRO LINE";
                        workSheet.Cells[5, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[5, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[5, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[5, j, 5, k].Merge = true;
                        workSheet.Cells[5, j].Value = item.ProductionLine == 1 ? "Card" : "Assembly";

                        workSheet.Cells[6, 1].Value = "TESTED BY";
                        workSheet.Cells[6, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[6, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[6, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[6, j, 6, k].Merge = true;

                        //objENOfficeMember = new enOfficeMember() { ID = item.TestedBy };
                        //objBLOfficeMember = new blOfficeMember(objENOfficeMember);
                        //objBLOfficeMember.Read();
                        obj = listOfOfficeMemeber.Find(x => x.ID == item.TestedBy);
                        workSheet.Cells[6, j].Value = obj.Name;

                        workSheet.Cells[7, 1].Value = "Card Serial Number";
                        workSheet.Cells[7, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[7, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[7, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[7, j, 7, k].Merge = true;
                        workSheet.Cells[7, j].Value = item.SerialCardNo;

                        workSheet.Cells[8, 1].Value = "System Rating";
                        workSheet.Cells[8, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[8, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[8, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[8, j, 8, k].Merge = true;
                        workSheet.Cells[8, j].Value = item.SystemRating;

                        workSheet.Cells[9, 1].Value = "MODEL";
                        workSheet.Cells[9, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[9, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[9, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[9, j, 9, k].Merge = true;
                        workSheet.Cells[9, j].Value = item.Model;

                        workSheet.Cells[10, 1].Value = "SYS SR NO";
                        workSheet.Cells[10, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[10, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[10, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[10, j, 10, k].Merge = true;
                        workSheet.Cells[10, j].Value = item.Barcode + "_" + item.QcStatus;
                        workSheet.Cells[10, j].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[10, j].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        workSheet.Column(j).AutoFit();

                        workSheet.Cells[11, 1].Value = "CON PROG NO.";
                        workSheet.Cells[11, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[11, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[11, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[11, j, 11, k].Merge = true;
                        workSheet.Cells[11, j].Value = item.ConProgNo;

                        workSheet.Cells[12, 1].Value = "DIS PROG NO.";
                        workSheet.Cells[12, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[12, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[12, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[12, j, 12, k].Merge = true;

                        var stringNumber = item.DisProgNo;
                        int numericValue;
                        bool isNumber = int.TryParse(stringNumber, out numericValue);
                        if (isNumber)
                        {
                            try
                            {
                                obj = listOfOfficeMemeber.Find(x => x.ID == numericValue);
                                workSheet.Cells[12, j].Value = obj.Name;
                            }
                            catch (Exception ex)
                            {
                                workSheet.Cells[12, j].Value = item.DisProgNo;
                            }

                        }
                        else
                        {
                            workSheet.Cells[12, j].Value = item.DisProgNo;
                        }

                        workSheet.Cells[13, 1].Value = "PARAMETERS";
                        workSheet.Cells[13, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[13, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                        workSheet.Cells[13, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                        workSheet.Cells[13, j].Value = "DISPLAY";
                        workSheet.Cells[13, j].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[13, j].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        workSheet.Cells[13, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        workSheet.Cells[13, j + 1].Value = "ACTUAL";
                        workSheet.Cells[13, j + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[13, j + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        workSheet.Cells[13, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        workSheet.Cells[13, j + 2].Value = "STATUS";
                        workSheet.Cells[13, j + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[13, j + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        workSheet.Cells[13, j + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        var m = 14;
                        foreach (var parm in item.listOfResponseSummary)
                        {
                            workSheet.Cells[m, 1].Value = parm.Parameters;
                            workSheet.Cells[m, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            workSheet.Cells[m, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                            workSheet.Cells[m, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                            workSheet.Cells[m, j].Value = parm.Dispaly;
                            workSheet.Cells[m, j + 1].Value = parm.Actual;
                            workSheet.Cells[m, j + 2].Value = parm.Status;
                            workSheet.Cells[m, j + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            m++;
                        }

                        i++;
                    }
                    var d = date.Replace('/', '-');
                    string p_strPath = excelPath + "\\" + d + ".xlsx";

                    if (System.IO.File.Exists(p_strPath))
                        System.IO.File.Delete(p_strPath);

                    FileStream objFileStrm = System.IO.File.Create(p_strPath);
                    objFileStrm.Close();

                    System.IO.File.WriteAllBytes(p_strPath, excel.GetAsByteArray());
                    excel.Dispose();
                    return Json("s", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
                }
                //Console.ReadKey();
            }
            return Json("s", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetResp()
        {
            var objENResponses = new enResponse();
            var objBLResponses = new blResponse(objENResponses);
            List<enResponse> listOfResponses = new List<enResponse>();
            try
            {
                listOfResponses = objBLResponses.ReadAllAndAggregate(null, null, null, null, null, typeof(enResponseSummary));
            }
            catch (Exception ex)
            {
            }
            return Json("s");
        }
    }
}