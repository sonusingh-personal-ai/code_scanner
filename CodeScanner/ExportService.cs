using BusinessLogicLayer;
using Entity;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CodeScanner
{
    public class ExportService
    {
        public void ExportAll()
        {
            Log.Info("ExportService ExportAll: Hangfire export started at " + DateTime.Now.ToString("o"));

            var objENResponse = new enResponse();
            var objBLResponse = new blResponse(objENResponse);

            List<enResponse> listOfResponses = new List<enResponse>();
            var todayString = DateTime.Today.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            try
            {
                Log.Info($"ExportService: Reading responses for today: {todayString}");

                objENResponse.CurrentDate = todayString;
                listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, null, null, null, typeof(enResponseSummary));

                Log.Info($"ExportService: Found {listOfResponses?.Count ?? 0} responses for today");
            }
            catch (Exception ex)
            {
                Log.Error("ExportService: Failed to read responses for today: " + ex);
                return;
            }

            if (listOfResponses == null || listOfResponses.Count == 0)
            {
                Log.Info("ExportService: No responses found for today : " + todayString + ". Nothing to export.");
                return;
            }

            // Gather office members for lookup
            var listOfOfficeMemeber = new List<enOfficeMember>();
            try
            {
                Log.Info("ExportService: Loading office members for name lookup");
                var objBLOfficeMember = new blOfficeMember(new enOfficeMember());
                listOfOfficeMemeber = objBLOfficeMember.ReadAll();
                Log.Info($"ExportService: Loaded {(listOfOfficeMemeber == null ? 0 : listOfOfficeMemeber.Count)} office members");
            }
            catch (Exception exOffice)
            {
                Log.Error("ExportService: Failed to load office members: " + exOffice);
            }

            // Convert office members to dictionary for O(1) fast lookup instead of List.Find O(N)
            var officeLookup = (listOfOfficeMemeber ?? new List<enOfficeMember>())
                .ToDictionary(x => x.ID, x => x);

            try
            {
                var excelPath = Utility.ApplicationSettings.getExcelPath;
                if (string.IsNullOrEmpty(excelPath))
                {
                    Log.Error("ExportService: Excel path not configured.");
                    return;
                }

                if (!Directory.Exists(excelPath))
                    Directory.CreateDirectory(excelPath);

                // Create files in chunks of 5000 records
                const int chunkSize = 5000;
                var datePart = DateTime.Now.ToString("yyyyMMdd");
                var total = listOfResponses.Count;
                var totalFiles = (int)Math.Ceiling((double)total / chunkSize);

                for (int fileIndex = 0; fileIndex < totalFiles; fileIndex++)
                {
                    var chunkStartIndex = fileIndex * chunkSize;
                    var chunk = listOfResponses.Skip(chunkStartIndex).Take(chunkSize).ToList();
                    var fileStart = chunkStartIndex + 1;
                    var fileEnd = chunkStartIndex + chunk.Count;
                    var fileNameChunk = totalFiles > 5000 ? $"auto_excel_{datePart}_file_{fileIndex + 1}_{fileStart}_{fileEnd}.xlsx" : $"auto_excel_{datePart}_{fileEnd}.xlsx";
                    var fullPathChunk = Path.Combine(excelPath, fileNameChunk);

                    try
                    {
                        if (File.Exists(fullPathChunk))
                        {
                            File.Delete(fullPathChunk);
                            Log.Info("ExportService: Existing export file deleted: " + fullPathChunk);
                        }
                    }
                    catch (Exception exDel)
                    {
                        Log.Error("ExportService: Failed to delete existing export file: " + exDel);
                    }


                    // Trackers for exact cell coordinates in case of error
                    int currentRow = 0;
                    int currentCol = 0;
                    string currentFieldLabel = "Initialization";

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

                        if (listOfResponses != null)
                        {
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
                                string qcStatusVal = item.QcStatus != null ? item.QcStatus.ToString() : "";
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
                        }

                        // AutoFit columns across worksheet dimension safely
                        if (workSheet.Dimension != null)
                        {
                            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                        }

                        // Ensure fullPathChunk is valid before saving
                        if (!string.IsNullOrWhiteSpace(fullPathChunk))
                        {
                            string directoryPath = Path.GetDirectoryName(fullPathChunk);
                            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            var bytes = excel.GetAsByteArray();
                            File.WriteAllBytes(fullPathChunk, bytes);

                            int responseCount = listOfResponses != null ? listOfResponses.Count : 0;
                            Log.Info("ExportService: Export saved to " + fullPathChunk + " with " + responseCount + " responses (chunk " + (fileIndex + 1) + "/" + totalFiles + ")");
                        }
                        else
                        {
                            Log.Error("ExportService: Export failed for chunk " + (fileIndex + 1) + "/" + totalFiles + " because fullPathChunk is null or empty.");
                        }
                    }
                }
            }
            catch (Exception exExcel)
            {
                Log.Error("ExportService: Failed to create export file: " + exExcel);
            }
        }
    }
}