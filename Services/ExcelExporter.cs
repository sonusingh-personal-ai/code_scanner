using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Entity;

namespace Services
{
    public static class ExcelExporter
    {
        // Build an Excel package (bytes) for the given responses and office members lookup.
        // Returns the file bytes which the caller can save or return to client.
        public static byte[] BuildExcelPackage(List<enResponse> listOfResponses, List<enOfficeMember> listOfOfficeMember)
        {
            if (listOfResponses == null || listOfResponses.Count == 0)
                return null;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var workSheet = package.Workbook.Worksheets.Add("Responses");
                workSheet.TabColor = System.Drawing.Color.Black;
                workSheet.DefaultRowHeight = 12;

                // header
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1].Value = "Testing Zig";

                // build a simple lookup for office members
                var officeLookup = (listOfOfficeMember ?? new List<enOfficeMember>()).ToDictionary(x => x.ID, x => x.Name);

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
                    workSheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                    workSheet.Cells[3, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[3, j, 3, k].Merge = true;
                    workSheet.Cells[3, j].Value = item.CreatedOn.ToString();

                    workSheet.Cells[4, 1].Value = "VISUAL";
                    workSheet.Cells[4, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[4, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                    workSheet.Cells[4, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[4, j, 4, k].Merge = true;
                    officeLookup.TryGetValue(item.VisualBy, out string visualName);
                    workSheet.Cells[4, j].Value = visualName ?? string.Empty;

                    workSheet.Cells[5, 1].Value = "PRO LINE";
                    workSheet.Cells[5, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[5, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                    workSheet.Cells[5, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[5, j, 5, k].Merge = true;
                    workSheet.Cells[5, j].Value = item.ProductionLine == 1 ? "Card" : "Assembly";

                    workSheet.Cells[6, 1].Value = "TESTED BY";
                    workSheet.Cells[6, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[6, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                    workSheet.Cells[6, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[6, j, 6, k].Merge = true;
                    officeLookup.TryGetValue(item.TestedBy, out string testedName);
                    workSheet.Cells[6, j].Value = testedName ?? string.Empty;

                    workSheet.Cells[7, 1].Value = "Card Serial Number";
                    workSheet.Cells[7, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[7, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                    workSheet.Cells[7, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[7, j, 7, k].Merge = true;
                    workSheet.Cells[7, j].Value = item.SerialCardNo;

                    workSheet.Cells[8, 1].Value = "System Rating";
                    workSheet.Cells[8, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[8, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                    workSheet.Cells[8, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[8, j, 8, k].Merge = true;
                    workSheet.Cells[8, j].Value = item.SystemRating;

                    workSheet.Cells[9, 1].Value = "MODEL";
                    workSheet.Cells[9, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[9, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LawnGreen);
                    workSheet.Cells[9, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[9, j, 9, k].Merge = true;
                    workSheet.Cells[9, j].Value = item.Model;

                    // write parameters if available
                    var summaryStartRow = 11;
                    if (item.listOfResponseSummary != null && item.listOfResponseSummary.Count > 0)
                    {
                        workSheet.Cells[10, j].Value = "PARAMETERS";
                        workSheet.Cells[10, j + 1].Value = "DISPLAY";
                        workSheet.Cells[10, j + 2].Value = "ACTUAL";
                        workSheet.Cells[10, j + 3].Value = "STATUS";

                        var r = summaryStartRow;
                        foreach (var s in item.listOfResponseSummary)
                        {
                            workSheet.Cells[r, j].Value = s.Parameters;
                            workSheet.Cells[r, j + 1].Value = s.Dispaly;
                            workSheet.Cells[r, j + 2].Value = s.Actual;
                            workSheet.Cells[r, j + 3].Value = s.Status;
                            r++;
                        }
                    }

                    i++;
                }

                if (workSheet.Dimension != null)
                {
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                }

                return package.GetAsByteArray();
            }
        }
    }
}
