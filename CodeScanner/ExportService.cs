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
            // Database stores CurrentDate in dd/MM/yyyy format (e.g. 13/09/2026).
            var todayString = DateTime.Today.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            try
            {
                Log.Info($"ExportService: Reading responses for today: {todayString}");

                objENResponse.CurrentDate = todayString;
                listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, null, null, null, typeof(enResponseSummary));

                // Fallback check: if no records found with dd/MM/yyyy, try M/d/yyyy in case records exist in that format
                if (listOfResponses == null || listOfResponses.Count == 0)
                {
                    var altTodayString = DateTime.Today.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    if (altTodayString != todayString)
                    {
                        objENResponse.CurrentDate = altTodayString;
                        listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, null, null, null, typeof(enResponseSummary));
                        if (listOfResponses != null && listOfResponses.Count > 0)
                        {
                            Log.Info($"ExportService: Found {listOfResponses.Count} responses using fallback format {altTodayString}");
                        }
                    }
                }

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

                    try
                    {
                        // Use shared Excel sheet generator
                        var bytes = Controllers.ExcelController.GenerateExcelSheet(chunk, listOfOfficeMemeber);
                        if (bytes == null || bytes.Length == 0)
                        {
                            Log.Error("ExportService: Generated excel is empty for chunk " + (fileIndex + 1));
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(fullPathChunk))
                        {
                            string directoryPath = Path.GetDirectoryName(fullPathChunk);
                            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            File.WriteAllBytes(fullPathChunk, bytes);

                            int responseCount = chunk.Count;
                            Log.Info("ExportService: Export saved to " + fullPathChunk + " with " + responseCount + " responses (chunk " + (fileIndex + 1) + "/" + totalFiles + ")");
                        }
                        else
                        {
                            Log.Error("ExportService: Export failed for chunk " + (fileIndex + 1) + "/" + totalFiles + " because fullPathChunk is null or empty.");
                        }
                    }
                    catch (Exception exChunk)
                    {
                        Log.Error("ExportService: Failed to export chunk " + (fileIndex + 1) + "/" + totalFiles + ": " + exChunk);
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