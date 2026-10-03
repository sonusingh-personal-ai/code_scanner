using BusinessLogicLayer;
using Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeScanner
{
    public class ExportService
    {
        // 1. Scheduled / Hangfire entry point (defaults to today's date)
        public void ExportAll()
        {
            var todayString = DateTime.Today.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            Log.Info($"ExportService ExportAll: Started for today ({todayString}) at " + DateTime.Now.ToString("o"));

            ExecuteExport(todayString, isAuto: true);
        }

        // 2. Direct On-Demand entry point (uses passed target date)
        public void ExportByDate(string targetDate)
        {
            Log.Info($"ExportService ExportByDate: Direct export requested for date '{targetDate}' at " + DateTime.Now.ToString("o"));

            if (string.IsNullOrWhiteSpace(targetDate))
            {
                Log.Error("ExportService ExportByDate: Target date was empty or null.");
                return;
            }

            ExecuteExport(targetDate, isAuto: false);
        }

        // 3. Shared Core Export Engine
        private void ExecuteExport(string targetDate, bool isAuto)
        {
            var objENResponse = new enResponse { CurrentDate = targetDate };
            var objBLResponse = new blResponse(objENResponse);
            List<enResponse> listOfResponses = new List<enResponse>();

            try
            {
                Log.Info($"ExportService: Reading responses for date: {targetDate}");
                listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, null, null, null, typeof(enResponseSummary));

                // Fallback format check for scheduled runs if initial format returns zero records
                if ((listOfResponses == null || listOfResponses.Count == 0) && isAuto)
                {
                    var altTodayString = DateTime.Today.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    if (altTodayString != targetDate)
                    {
                        objENResponse.CurrentDate = altTodayString;
                        listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, null, null, null, typeof(enResponseSummary));
                        if (listOfResponses != null && listOfResponses.Count > 0)
                        {
                            Log.Info($"ExportService: Found {listOfResponses.Count} responses using fallback format {altTodayString}");
                        }
                    }
                }

                Log.Info($"ExportService: Found {listOfResponses?.Count ?? 0} responses for {targetDate}");
            }
            catch (Exception ex)
            {
                Log.Error($"ExportService: Failed to read responses for date {targetDate}: " + ex);
                return;
            }

            if (listOfResponses == null || listOfResponses.Count == 0)
            {
                Log.Info($"ExportService: No responses found for date {targetDate}. Nothing to export.");
                return;
            }

            // Load Office Members lookup once
            var listOfOfficeMember = new List<enOfficeMember>();
            try
            {
                var objBLOfficeMember = new blOfficeMember(new enOfficeMember());
                listOfOfficeMember = objBLOfficeMember.ReadAll() ?? new List<enOfficeMember>();
            }
            catch (Exception exOffice)
            {
                Log.Error("ExportService: Failed to load office members: " + exOffice);
            }

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

                // Chunk & File Output Logic
                const int chunkSize = 5000;
                string datePart = targetDate.Replace("/", "").Replace("-", "");
                string prefix = isAuto ? "auto_excel" : "manual_excel";

                var total = listOfResponses.Count;
                var totalFiles = (int)Math.Ceiling((double)total / chunkSize);

                for (int fileIndex = 0; fileIndex < totalFiles; fileIndex++)
                {
                    var chunkStartIndex = fileIndex * chunkSize;
                    var chunk = listOfResponses.Skip(chunkStartIndex).Take(chunkSize).ToList();
                    var fileStart = chunkStartIndex + 1;
                    var fileEnd = chunkStartIndex + chunk.Count;

                    var fileNameChunk = totalFiles > 1
                        ? $"{prefix}_{datePart}_file_{fileIndex + 1}_{fileStart}_{fileEnd}.xlsx"
                        : $"{prefix}_{datePart}_{fileEnd}.xlsx";

                    var fullPathChunk = Path.Combine(excelPath, fileNameChunk);

                    try
                    {
                        if (File.Exists(fullPathChunk))
                            File.Delete(fullPathChunk);

                        var bytes = Controllers.ExcelController.GenerateExcelSheet(chunk, listOfOfficeMember);
                        if (bytes == null || bytes.Length == 0)
                        {
                            Log.Error($"ExportService: Empty Excel generated for chunk {fileIndex + 1}");
                            continue;
                        }

                        File.WriteAllBytes(fullPathChunk, bytes);
                        Log.Info($"ExportService: Export saved to {fullPathChunk} ({chunk.Count} records)");
                    }
                    catch (Exception exChunk)
                    {
                        Log.Error($"ExportService: Failed processing chunk {fileIndex + 1}/{totalFiles}: " + exChunk);
                    }
                }
            }
            catch (Exception exExcel)
            {
                Log.Error("ExportService: Failed generating export files: " + exExcel);
            }
        }
    }
}