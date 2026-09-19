using OfficeOpenXml;
using OfficeOpenXml.Style;
using BusinessLogicLayer;
using Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;


namespace CodeScanner.Controllers
{
    public class ExcelController : Controller
    {
        [HttpPost]
        public JsonResult Download(List<int> ids)
        {
            // 1. Validate Input Parameter
            if (ids == null || ids.Count == 0)
            {
                return Json("No IDs provided for download.", JsonRequestBehavior.AllowGet);
            }

            // 2. Validate Storage Path
            var excelPath = Utility.ApplicationSettings.getExcelPath;
            if (string.IsNullOrWhiteSpace(excelPath) || !Directory.Exists(excelPath))
            {
                return Json("File Path Not Exist", JsonRequestBehavior.AllowGet);
            }

            var listOfResponses = new List<enResponse>();

            try
            {
                var objENResponse = new enResponse();
                var objBLResponse = new blResponse(objENResponse);

                var allResponses = objBLResponse.ReadAll();
                if (allResponses != null)
                {
                    listOfResponses = allResponses.Where(x => x != null && ids.Contains(x.Id)).ToList();
                }

                foreach (var response in listOfResponses)
                {
                    if (response == null) continue;

                    try
                    {
                        var objBLResponseSummary = new blResponseSummary(new enResponseSummary() { ResponseId = response.Id });
                        response.listOfResponseSummary = objBLResponseSummary.ReadAll() ?? new List<enResponseSummary>();
                    }
                    catch
                    {
                        response.listOfResponseSummary = new List<enResponseSummary>();
                    }
                }
            }
            catch (Exception ex)
            {
                return Json("Data Fetch Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }

            if (listOfResponses.Count == 0)
            {
                return Json("No matching records found.", JsonRequestBehavior.AllowGet);
            }

            // 3. Fetch Office Members Safely
            var listOfOfficeMemeber = new List<enOfficeMember>();
            try
            {
                var objBLOfficeMember = new blOfficeMember(new enOfficeMember());
                listOfOfficeMemeber = objBLOfficeMember.ReadAll() ?? new List<enOfficeMember>();
            }
            catch (Exception ex)
            {
                Log.Error("Error fetching office members: " + ex.Message);
            }

            try
            {
                var bytes = GenerateExcelSheet(listOfResponses, listOfOfficeMemeber);
                if (bytes == null || bytes.Length == 0)
                {
                    return Json("Generated excel is empty.", JsonRequestBehavior.AllowGet);
                }

                // File Creation & Writing
                var datePart = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string p_strPath = System.IO.Path.Combine(excelPath, $"excel_{datePart}.xlsx");

                System.IO.File.WriteAllBytes(p_strPath, bytes);

                return Json("s", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error("Error in ExportToExcel: " + ex.Message);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public static byte[] generateExcelSheet(List<enResponse> listOfResponses, List<enOfficeMember> listOfOfficeMemeber)
        {
            return GenerateExcelSheet(listOfResponses, listOfOfficeMemeber);
        }

        public static byte[] GenerateExcelSheet(List<enResponse> listOfResponses, List<enOfficeMember> listOfOfficeMemeber)
        {
            if (listOfResponses == null || listOfResponses.Count == 0)
            {
                return null;
            }

            // Trackers for exact cell coordinates in case of error
            int currentRow = 0;
            int currentCol = 0;
            string currentFieldLabel = "Initialization";

            try
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    // Clean sheet name (Excel worksheet names must be <= 31 chars and non-null/non-empty)
                    string rawModelName = (listOfResponses != null && listOfResponses.Count > 0 && listOfResponses[0] != null)
                        ? listOfResponses[0].Model
                        : null;

                    string sheetName = string.IsNullOrWhiteSpace(rawModelName) ? "Sheet1" : rawModelName.Trim();
                    if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);

                    var workSheet = excel.Workbook.Worksheets.Add(sheetName);
                    workSheet.TabColor = System.Drawing.Color.Black;
                    workSheet.DefaultRowHeight = 12;

                    workSheet.Row(1).Height = 20;
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(1).Style.Font.Bold = true;

                    currentRow = 1; currentCol = 1; currentFieldLabel = "Header: Testing Zig";
                    workSheet.Cells[1, 1].Value = "Testing Zig";

                    var i = 1;
                    var j = 2;
                    var k = 4;

                    foreach (var item in listOfResponses)
                    {
                        if (item == null) continue;

                        if (i > 1)
                        {
                            j = j + 3;
                            k = k + 3;
                        }

                        // DATE & TIME
                        currentRow = 3; currentCol = 1; currentFieldLabel = "DATE & TIME Label";
                        workSheet.Cells[3, 1].Value = "DATE & TIME";
                        workSheet.Cells[3, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "DATE & TIME Value";
                        workSheet.Cells[3, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[3, j, 3, k].Merge = true;
                        workSheet.Cells[3, j].Value = item.CreatedOn != null ? item.CreatedOn.ToString() : "";

                        // VISUAL
                        currentRow = 4; currentCol = 1; currentFieldLabel = "VISUAL Label";
                        workSheet.Cells[4, 1].Value = "VISUAL";
                        workSheet.Cells[4, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[4, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "VISUAL Value";
                        workSheet.Cells[4, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[4, j, 4, k].Merge = true;

                        var visualObj = listOfOfficeMemeber != null
                            ? listOfOfficeMemeber.FirstOrDefault(x => x != null && x.ID == item.VisualBy)
                            : null;
                        workSheet.Cells[4, j].Value = (visualObj != null && !string.IsNullOrEmpty(visualObj.Name)) ? visualObj.Name : "";

                        // PRO LINE
                        currentRow = 5; currentCol = 1; currentFieldLabel = "PRO LINE Label";
                        workSheet.Cells[5, 1].Value = "PRO LINE";
                        workSheet.Cells[5, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[5, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "PRO LINE Value";
                        workSheet.Cells[5, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[5, j, 5, k].Merge = true;
                        workSheet.Cells[5, j].Value = item.ProductionLine == 1 ? "Card" : (item.ProductionLine == 2 ? "Assembly" : "");

                        // TESTED BY
                        currentRow = 6; currentCol = 1; currentFieldLabel = "TESTED BY Label";
                        workSheet.Cells[6, 1].Value = "TESTED BY";
                        workSheet.Cells[6, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[6, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "TESTED BY Value";
                        workSheet.Cells[6, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[6, j, 6, k].Merge = true;

                        var testedObj = listOfOfficeMemeber != null
                            ? listOfOfficeMemeber.FirstOrDefault(x => x != null && x.ID == item.TestedBy)
                            : null;
                        workSheet.Cells[6, j].Value = (testedObj != null && !string.IsNullOrEmpty(testedObj.Name)) ? testedObj.Name : "";

                        // Card Serial Number
                        currentRow = 7; currentCol = 1; currentFieldLabel = "Card Serial Number Label";
                        workSheet.Cells[7, 1].Value = "Card Serial Number";
                        workSheet.Cells[7, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[7, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "Card Serial Number Value";
                        workSheet.Cells[7, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[7, j, 7, k].Merge = true;
                        workSheet.Cells[7, j].Value = item.SerialCardNo ?? "";

                        // System Rating
                        currentRow = 8; currentCol = 1; currentFieldLabel = "System Rating Label";
                        workSheet.Cells[8, 1].Value = "System Rating";
                        workSheet.Cells[8, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[8, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "System Rating Value";
                        workSheet.Cells[8, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[8, j, 8, k].Merge = true;
                        workSheet.Cells[8, j].Value = item.SystemRating ?? "";

                        // MODEL
                        currentRow = 9; currentCol = 1; currentFieldLabel = "MODEL Label";
                        workSheet.Cells[9, 1].Value = "MODEL";
                        workSheet.Cells[9, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[9, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "MODEL Value";
                        workSheet.Cells[9, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[9, j, 9, k].Merge = true;
                        workSheet.Cells[9, j].Value = item.Model ?? "";

                        // SYS SR NO
                        currentRow = 10; currentCol = 1; currentFieldLabel = "SYS SR NO Label";
                        workSheet.Cells[10, 1].Value = "SYS SR NO";
                        workSheet.Cells[10, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[10, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "SYS SR NO Value";
                        workSheet.Cells[10, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[10, j, 10, k].Merge = true;
                        string barcodeVal = item.Barcode ?? "";
                        string qcStatusVal = item.QcStatus.ToString();
                        workSheet.Cells[10, j].Value = (string.IsNullOrEmpty(barcodeVal) && string.IsNullOrEmpty(qcStatusVal)) ? "" : $"{barcodeVal}_{qcStatusVal}";
                        workSheet.Cells[10, j].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[10, j].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        workSheet.Column(j).AutoFit();

                        // CON PROG NO
                        currentRow = 11; currentCol = 1; currentFieldLabel = "CON PROG NO. Label";
                        workSheet.Cells[11, 1].Value = "CON PROG NO.";
                        workSheet.Cells[11, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[11, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "CON PROG NO. Value";
                        workSheet.Cells[11, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[11, j, 11, k].Merge = true;
                        workSheet.Cells[11, j].Value = item.ConProgNo ?? "";

                        // DIS PROG NO
                        currentRow = 12; currentCol = 1; currentFieldLabel = "DIS PROG NO. Label";
                        workSheet.Cells[12, 1].Value = "DIS PROG NO.";
                        workSheet.Cells[12, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[12, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "DIS PROG NO. Value";
                        workSheet.Cells[12, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[12, j, 12, k].Merge = true;

                        // 1. Convert DisProgNo to string safely regardless of whether it's string, int, or nullable int
                        string disProgStr = item.DisProgNo != null ? item.DisProgNo.ToString() : "";

                        // 2. Variable declaration for backward-compatible C# versions
                        int numericValue;
                        if (!string.IsNullOrWhiteSpace(disProgStr) && int.TryParse(disProgStr, out numericValue))
                        {
                            // Match ID safely with null checks
                            var disProgObj = listOfOfficeMemeber != null
                                ? listOfOfficeMemeber.FirstOrDefault(x => x != null && x.ID == numericValue)
                                : null;

                            workSheet.Cells[12, j].Value = (disProgObj != null && !string.IsNullOrEmpty(disProgObj.Name))
                                                            ? disProgObj.Name
                                                            : disProgStr;
                        }
                        else
                        {
                            workSheet.Cells[12, j].Value = disProgStr;
                        }

                        // PARAMETERS Headers
                        currentRow = 13; currentCol = 1; currentFieldLabel = "PARAMETERS Label";
                        workSheet.Cells[13, 1].Value = "PARAMETERS";
                        workSheet.Cells[13, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[13, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);

                        currentCol = j; currentFieldLabel = "DISPLAY Header";
                        workSheet.Cells[13, j].Value = "DISPLAY";
                        workSheet.Cells[13, j].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[13, j].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        workSheet.Cells[13, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        currentCol = j + 1; currentFieldLabel = "ACTUAL Header";
                        workSheet.Cells[13, j + 1].Value = "ACTUAL";
                        workSheet.Cells[13, j + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[13, j + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        workSheet.Cells[13, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        currentCol = j + 2; currentFieldLabel = "STATUS Header";
                        workSheet.Cells[13, j + 2].Value = "STATUS";
                        workSheet.Cells[13, j + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheet.Cells[13, j + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        workSheet.Cells[13, j + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // Parameter Rows
                        var m = 14;
                        if (item.listOfResponseSummary != null)
                        {
                            foreach (var parm in item.listOfResponseSummary)
                            {
                                if (parm == null) continue;

                                currentRow = m; currentCol = 1; currentFieldLabel = $"Parameters ({parm.Parameters ?? ""})";
                                workSheet.Cells[m, 1].Value = parm.Parameters ?? "";
                                workSheet.Cells[m, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                workSheet.Cells[m, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                                workSheet.Cells[m, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                currentCol = j; currentFieldLabel = $"Display Value ({parm.Dispaly ?? ""})";
                                workSheet.Cells[m, j].Value = parm.Dispaly ?? "";

                                currentCol = j + 1; currentFieldLabel = $"Actual Value ({parm.Actual ?? ""})";
                                workSheet.Cells[m, j + 1].Value = parm.Actual ?? "";

                                currentCol = j + 2; currentFieldLabel = $"Status Value ({parm.Status ?? ""})";
                                workSheet.Cells[m, j + 2].Value = parm.Status ?? "";
                                workSheet.Cells[m, j + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                m++;
                            }
                        }

                        i++;
                    }

                    // AutoFit columns across worksheet dimension safely
                    if (workSheet.Dimension != null)
                    {
                        workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                    }

                    return excel.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                string errorDetails = $"Failed at Field: [{currentFieldLabel}] | Cell Address: Row {currentRow}, Column {currentCol} | Exception: {ex.Message}";
                Log.Error("Error in GenerateExcelSheet: " + errorDetails);
                throw new Exception(errorDetails, ex);
            }
        }
    }
}