using BusinessLogicLayer;
using Entity;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            try
            {
                // Export only today's responses
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(1);
                Log.Info($"ExportService: Reading responses from {startDate:o} to {endDate:o}");
                listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, startDate, endDate, null, typeof(enResponseSummary));
                Log.Info("ExportService: Total Reponses : " + listOfResponses.Count());
            }
            catch (Exception ex)
            {
                Log.Error("ExportService: Failed to read responses for today: " + ex);
            }

            if (listOfResponses == null || listOfResponses.Count == 0)
            {
                try
                {
                    Log.Info("ExportService: No responses found for today - falling back to last 50,000 responses");
                    var all = objBLResponse.ReadAllAndAggregate(1, 10, null, null, null, typeof(enResponseSummary));
                    if (all != null && all.Count > 0)
                    {
                        listOfResponses = all.OrderByDescending(x => x.CreatedOn).Take(50000).ToList();
                        Log.Info($"ExportService: Fallback selected {listOfResponses.Count} responses (last by CreatedOn)");
                    }
                    else
                    {
                        Log.Info("ExportService: No responses available to export (even in fallback). Aborting.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ExportService: Failed during fallback read: " + ex);
                    return;
                }
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

                    using (var excel = new ExcelPackage())
                    {
                        string sheetName = !string.IsNullOrWhiteSpace(chunk[0].Model)
                            ? chunk[0].Model
                            : "Responses";

                        var workSheet = excel.Workbook.Worksheets.Add(sheetName);
                        workSheet.TabColor = System.Drawing.Color.Black;
                        workSheet.DefaultRowHeight = 12;

                        // Title row
                        workSheet.Row(1).Height = 20;
                        workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Row(1).Style.Font.Bold = true;
                        workSheet.Cells[1, 1].Value = "Testing Zig - Daily Export";

                        // Left header column (Row headers with LawnGreen background)
                        workSheet.Cells[3, 1].Value = "DATE & TIME";
                        workSheet.Cells[4, 1].Value = "VISUAL";
                        workSheet.Cells[5, 1].Value = "PRO LINE";
                        workSheet.Cells[6, 1].Value = "TESTED BY";
                        workSheet.Cells[7, 1].Value = "Card Serial Number";
                        workSheet.Cells[8, 1].Value = "System Rating";
                        workSheet.Cells[9, 1].Value = "MODEL";
                        workSheet.Cells[10, 1].Value = "SYS SR NO";
                        workSheet.Cells[11, 1].Value = "CON PROG NO.";
                        workSheet.Cells[12, 1].Value = "DIS PROG NO.";
                        workSheet.Cells[13, 1].Value = "PARAMETERS";

                        for (int headerRow = 3; headerRow <= 13; headerRow++)
                        {
                            workSheet.Cells[headerRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[headerRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                            workSheet.Cells[headerRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        var i = 1;
                        var j = 2;
                        var k = 4;

                        foreach (var item in chunk)
                        {
                            if (i > 1)
                            {
                                j += 3;
                                k += 3;
                            }

                            // 1. DATE & TIME
                            workSheet.Cells[3, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[3, j, 3, k].Merge = true;
                            workSheet.Cells[3, j].Value = item.CreatedOn.ToString();

                            // 2. VISUAL
                            workSheet.Cells[4, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[4, j, 4, k].Merge = true;
                            enOfficeMember visualObj;
                            officeLookup.TryGetValue(item.VisualBy, out visualObj);
                            workSheet.Cells[4, j].Value = visualObj == null ? "" : visualObj.Name;

                            // 3. PRO LINE
                            workSheet.Cells[5, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[5, j, 5, k].Merge = true;
                            workSheet.Cells[5, j].Value = item.ProductionLine == 1 ? "Card" : "Assembly";

                            // 4. TESTED BY
                            workSheet.Cells[6, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[6, j, 6, k].Merge = true;
                            enOfficeMember testedObj;
                            officeLookup.TryGetValue(item.TestedBy, out testedObj);
                            workSheet.Cells[6, j].Value = testedObj == null ? "" : testedObj.Name;

                            // 5. CARD SERIAL NUMBER
                            workSheet.Cells[7, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[7, j, 7, k].Merge = true;
                            workSheet.Cells[7, j].Value = item.SerialCardNo;

                            // 6. SYSTEM RATING
                            workSheet.Cells[8, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[8, j, 8, k].Merge = true;
                            workSheet.Cells[8, j].Value = item.SystemRating;

                            // 7. MODEL
                            workSheet.Cells[9, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[9, j, 9, k].Merge = true;
                            workSheet.Cells[9, j].Value = item.Model;

                            // 8. SYS SR NO
                            workSheet.Cells[10, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[10, j, 10, k].Merge = true;
                            workSheet.Cells[10, j].Value = item.Barcode + "_" + item.QcStatus;
                            workSheet.Cells[10, j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[10, j].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

                            // 9. CON PROG NO.
                            workSheet.Cells[11, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[11, j, 11, k].Merge = true;
                            workSheet.Cells[11, j].Value = item.ConProgNo;

                            // 10. DIS PROG NO.
                            workSheet.Cells[12, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            workSheet.Cells[12, j, 12, k].Merge = true;
                            int numericValue;
                            enOfficeMember disProgMember;
                            if (int.TryParse(item.DisProgNo, out numericValue) && officeLookup.TryGetValue(numericValue, out disProgMember))
                            {
                                workSheet.Cells[12, j].Value = disProgMember.Name;
                            }
                            else
                            {
                                workSheet.Cells[12, j].Value = item.DisProgNo;
                            }

                            // 11. PARAMETERS HEADER (Row 13)
                            workSheet.Cells[13, j].Value = "DISPLAY";
                            workSheet.Cells[13, j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[13, j].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                            workSheet.Cells[13, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            workSheet.Cells[13, j + 1].Value = "ACTUAL";
                            workSheet.Cells[13, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[13, j + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                            workSheet.Cells[13, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            workSheet.Cells[13, j + 2].Value = "STATUS";
                            workSheet.Cells[13, j + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[13, j + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                            workSheet.Cells[13, j + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            // 12. PARAMETERS VALUES (Rows 14+)
                            if (item.listOfResponseSummary != null && item.listOfResponseSummary.Count > 0)
                            {
                                var m = 14;
                                foreach (var parm in item.listOfResponseSummary)
                                {
                                    workSheet.Cells[m, 1].Value = parm.Parameters;
                                    workSheet.Cells[m, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    workSheet.Cells[m, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                                    workSheet.Cells[m, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                    workSheet.Cells[m, j].Value = parm.Dispaly;
                                    workSheet.Cells[m, j + 1].Value = parm.Actual;
                                    workSheet.Cells[m, j + 2].Value = parm.Status;
                                    workSheet.Cells[m, j + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                    m++;
                                }
                            }

                            i++;
                        }

                        if (workSheet.Dimension != null)
                        {
                            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                        }

                        var bytes = excel.GetAsByteArray();
                        File.WriteAllBytes(fullPathChunk, bytes);
                        Log.Info("ExportService: Export saved to " + fullPathChunk + " with " + chunk.Count + " responses (chunk " + (fileIndex + 1) + "/" + totalFiles + ")");
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