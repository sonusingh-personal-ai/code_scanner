using BusinessLogicLayer;
using Entity;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Services
{
    public class ExportService
    {
        public void ExportAll()
        {
            Utility.Log.Info("ExportService: Hangfire export started at " + DateTime.Now.ToString("o"));

            var objENResponse = new enResponse();
            var objBLResponse = new blResponse(objENResponse);

            List<enResponse> listOfResponses = new List<enResponse>();
            try
            {
                // export only today's responses
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(1);
                Utility.Log.Info($"ExportService: Reading responses from {startDate:o} to {endDate:o}");
                listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, startDate, endDate, null, typeof(enResponseSummary));
            }
            catch (Exception ex)
            {
                Utility.Log.Error("ExportService: Failed to read responses for today: " + ex);
                // continue to try fallback
            }

            if (listOfResponses == null || listOfResponses.Count == 0)
            {
                try
                {
                    Utility.Log.Info("ExportService: No responses found for today - falling back to last 5 responses");
                    // explicitly request up to 50000 rows to avoid DB default paging limits
                    var all = objBLResponse.ReadAllAndAggregate(null, null, null, null, null, typeof(enResponseSummary));
                    if (all != null && all.Count > 0)
                    {
                        listOfResponses = all.OrderByDescending(x => x.CreatedOn).Take(500).ToList();
                        Utility.Log.Info($"ExportService: Fallback selected {listOfResponses.Count} responses (last by CreatedOn)");
                    }
                    else
                    {
                        Utility.Log.Info("ExportService: No responses available to export (even in fallback). Aborting.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Utility.Log.Error("ExportService: Failed during fallback read: " + ex);
                    return;
                }
            }

            // gather office members for name lookups
            var listOfOfficeMemeber = new List<enOfficeMember>();
            try
            {
                Utility.Log.Info("ExportService: Loading office members for name lookup");
                var objBLOfficeMember = new blOfficeMember(new enOfficeMember());
                listOfOfficeMemeber = objBLOfficeMember.ReadAll();
                Utility.Log.Info($"ExportService: Loaded {(listOfOfficeMemeber == null ? 0 : listOfOfficeMemeber.Count)} office members");
            }
            catch { }

            try
            {
                var excelPath = Utility.ApplicationSettings.getExcelPath;
                if (string.IsNullOrEmpty(excelPath))
                {
                    Utility.Log.Error("ExportService: Excel path not configured.");
                    return;
                }

                if (!Directory.Exists(excelPath))
                    Directory.CreateDirectory(excelPath);

                // create file name with timestamp
                var fileName = $"Responses_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fullPath = Path.Combine(excelPath, fileName);
                // Ensure each response has its full listOfResponseSummary populated
                try
                {
                    foreach (var item in listOfResponses)
                    {
                        try
                        {
                            var blSummary = new blResponseSummary(new enResponseSummary() { ResponseId = item.Id });
                            item.listOfResponseSummary = blSummary.ReadAll() ?? new List<enResponseSummary>();
                        }
                        catch (Exception ex)
                        {
                            Utility.Log.Error("ExportService: Failed to load summaries for ResponseId=" + item.Id + " : " + ex.Message);
                            item.listOfResponseSummary = new List<enResponseSummary>();
                        }
                    }
                }
                catch { }

                // Use shared Excel exporter to generate bytes so manual and scheduled exports match
                try
                {
                    // Use the controller's public wrapper so both manual and scheduled exports go through the same codepath
                    var bytes = CodeScanner.Controllers.ExcelController.generateExcelSheet(listOfResponses, listOfOfficeMemeber);
                    if (bytes == null || bytes.Length == 0)
                    {
                        Utility.Log.Error("ExportService: Generated excel is empty. Aborting.");
                        return;
                    }

                    File.WriteAllBytes(fullPath, bytes);
                    Utility.Log.Info($"ExportService: Export saved to {fullPath} with {listOfResponses.Count} responses");
                }
                catch (Exception ex)
                {
                    Utility.Log.Error("ExportService: Failed to create excel: " + ex);
                }
            }
            catch (Exception ex)
            {
                Utility.Log.Error("ExportService: Failed to create excel: " + ex);
            }
        }

        private string EscapeCsv(string s)
        {
            if (s == null) return string.Empty;
            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            {
                return '"' + s.Replace("\"", "\"\"") + '"';
            }
            return s;
        }
    }
}
