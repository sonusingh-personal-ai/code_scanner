using CodeScanner.Application.Common.Interfaces;
using CodeScanner.Application.DTOs;
using CodeScanner.Application.Services;
using CodeScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeScanner.Infrastructure.Services;

public class TestingService : ITestingService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TestingService> _logger;

    public TestingService(
        IApplicationDbContext context,
        ILogger<TestingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TestResultDto> ProcessRawTelemetryAsync(string rawTelemetry, SaveResponseRequestDto context)
    {
        var result = new TestResultDto
        {
            Barcode = context.Barcode,
            Timestamp = DateTime.Now
        };

        if (string.IsNullOrWhiteSpace(rawTelemetry))
        {
            result.IsOverallPass = false;
            result.Message = "Empty telemetry data received";
            return result;
        }

        string[] tokens = rawTelemetry.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .ToArray();

        if (tokens.Length < 5)
        {
            result.IsOverallPass = false;
            result.Message = "Invalid telemetry packet format: missing headers or footer";
            return result;
        }

        result.Header = tokens[0];
        result.DisplayProgNo = tokens[1];
        result.ControlProgNo = tokens[2];
        result.SystemRating = tokens[3];
        result.Footer = tokens[^1];

        // Fetch Setting & SettingInfo by Model Name or FileId
        Setting? setting = null;
        if (!string.IsNullOrEmpty(context.Model))
        {
            setting = await _context.Settings
                .Include(s => s.SettingInfos)
                .FirstOrDefaultAsync(s => s.FileId == context.Model);
        }

        // Align parameters
        var alignedParams = AlignParameters(setting, tokens);
        result.Parameters = alignedParams;

        // Determine Pass / Fail: overall pass if every parameter is OK or PASS
        bool allOk = alignedParams.Count > 0 && alignedParams.All(p => p.IsPass);
        result.IsOverallPass = allOk;
        int qcStatusVal = allOk ? 1 : 2; // 1: Pass, 2: Fail

        result.ModelName = context.Model ?? (setting != null ? setting.FileId : string.Empty);

        // Save Response record
        var now = DateTime.Now;
        var responseEntity = new Response
        {
            Barcode = string.IsNullOrWhiteSpace(context.Barcode) ? $"SIM-{now:yyyyMMddHHmmss}" : context.Barcode,
            QcStatus = qcStatusVal,
            VisualBy = context.VisualBy,
            TestedBy = context.TestedBy,
            ProductionLine = context.ProductionLine ?? 1,
            ProcessEngg = context.ProcessEngg,
            SerialCardNo = context.SerialCardNo,
            Model = result.ModelName,
            ConProgNo = result.ControlProgNo,
            DisProgNo = result.DisplayProgNo,
            SystemRating = result.SystemRating,
            CurrentDate = now.ToString("dd-MM-yyyy"),
            CurrentTime = now.ToString("HH:mm:ss"),
            ResponseTime = now,
            CreatedOn = now,
            PrinterModelId = context.PrinterModelId,
            Line = context.Line
        };

        _context.Responses.Add(responseEntity);
        await _context.SaveChangesAsync();

        result.ResponseId = responseEntity.Id;

        // Save ResponseSummary records
        foreach (var item in alignedParams)
        {
            var summary = new ResponseSummary
            {
                ResponseId = responseEntity.Id,
                Parameters = item.Parameter,
                Display = item.Display,
                Actual = item.Actual,
                Status = item.Status,
                IsFinal = allOk
            };
            _context.ResponseSummaries.Add(summary);
        }
        await _context.SaveChangesAsync();

        // Format QR Code String
        result.FormattedQrCode = GenerateQrCodeString(responseEntity, result.Parameters);
        result.Message = allOk ? "Test Cycle Completed: PASS" : "Test Cycle Completed: FAIL";

        return result;
    }

    public async Task<TestResultDto> SimulateTestCycleAsync(SaveResponseRequestDto context)
    {
        // Lookup setting infos for active model
        var settingInfos = await _context.SettingInfos
            .Where(s => s.Setting != null && s.Setting.FileId == context.Model)
            .OrderBy(s => s.Id)
            .ToListAsync();

        if (settingInfos.Count == 0)
        {
            // Fallback default parameters
            settingInfos = await _context.SettingInfos.Take(22).ToListAsync();
        }

        var sb = new List<string>
        {
            "@", // Header
            context.DisProgNo ?? "DP-101",
            context.ConProgNo ?? "CP-202",
            context.SystemRating ?? "12V-850VA"
        };

        var rand = new Random();
        foreach (var si in settingInfos)
        {
            // Simulate 95% pass rate
            bool pass = rand.NextDouble() > 0.05;
            int dispVal = rand.Next(100, 240);
            int actVal = pass ? dispVal + rand.Next(-2, 3) : dispVal + 25;
            string status = pass ? "OK" : "FAIL";

            sb.Add($"{dispVal}:{actVal}:{status}");
        }

        sb.Add("^"); // Footer

        string rawTelemetry = string.Join(",", sb);
        return await ProcessRawTelemetryAsync(rawTelemetry, context);
    }

    private static List<TelemetryParamDto> AlignParameters(Setting? setting, string[] tokens)
    {
        var list = new List<TelemetryParamDto>();
        if (tokens.Length < 5) return list;

        var settingInfos = setting?.SettingInfos?.OrderBy(s => s.Id).ToList() ?? new List<SettingInfo>();

        // Tokens between index 4 and length - 2 are parameters
        var paramTokens = new List<string>();
        for (int i = 4; i < tokens.Length - 1; i++)
        {
            paramTokens.Add(tokens[i]);
        }

        // 1. Exact count match
        if (paramTokens.Count == settingInfos.Count)
        {
            for (int i = 0; i < paramTokens.Count; i++)
            {
                list.Add(ParseTelemetryItem(paramTokens[i], settingInfos[i].Parameters));
            }
            return list;
        }

        // 2. Tolerance alignment: FAN discrepancy handling
        int telFanIdx = paramTokens.FindIndex(t => !string.IsNullOrEmpty(t) && t.Contains("FAN", StringComparison.OrdinalIgnoreCase));
        int setFanIdx = settingInfos.FindIndex(s => !string.IsNullOrEmpty(s.Parameters) && s.Parameters.Contains("FAN", StringComparison.OrdinalIgnoreCase));

        if (telFanIdx >= 0 && setFanIdx < 0 && paramTokens.Count == settingInfos.Count + 1)
        {
            int setIdx = 0;
            for (int telIdx = 0; telIdx < paramTokens.Count; telIdx++)
            {
                if (telIdx == telFanIdx)
                {
                    list.Add(ParseTelemetryItem(paramTokens[telIdx], "FAN (CARD SECTION)"));
                }
                else
                {
                    if (setIdx < settingInfos.Count)
                    {
                        list.Add(ParseTelemetryItem(paramTokens[telIdx], settingInfos[setIdx].Parameters));
                        setIdx++;
                    }
                }
            }
            return list;
        }

        // 3. Fallback zip alignment
        int min = Math.Min(paramTokens.Count, settingInfos.Count);
        for (int i = 0; i < min; i++)
        {
            list.Add(ParseTelemetryItem(paramTokens[i], settingInfos[i].Parameters));
        }

        for (int i = min; i < paramTokens.Count; i++)
        {
            list.Add(ParseTelemetryItem(paramTokens[i], $"PARAM {i + 1}"));
        }

        for (int i = min; i < settingInfos.Count; i++)
        {
            list.Add(new TelemetryParamDto
            {
                Parameter = settingInfos[i].Parameters,
                Display = "--",
                Actual = "--",
                Status = "N/A"
            });
        }

        return list;
    }

    private static TelemetryParamDto ParseTelemetryItem(string rawToken, string parameterName)
    {
        var item = new TelemetryParamDto
        {
            Parameter = parameterName
        };

        if (string.IsNullOrWhiteSpace(rawToken))
        {
            item.Status = "N/A";
            return item;
        }

        if (rawToken.Contains(':'))
        {
            string[] parts = rawToken.Split(':');
            item.Display = parts.Length > 0 ? parts[0] : "--";
            item.Actual = parts.Length > 1 ? parts[1] : "--";
            item.Status = parts.Length > 2 ? parts[2] : (parts.Length > 1 ? parts[1] : "OK");
        }
        else
        {
            item.Status = rawToken;
            item.Display = rawToken;
            item.Actual = rawToken;
        }

        return item;
    }

    private static string GenerateQrCodeString(Response response, List<TelemetryParamDto> parameters)
    {
        return $"Barcode:{response.Barcode};Model:{response.Model};Date:{response.CurrentDate};Time:{response.CurrentTime};QC:{(response.QcStatus == 1 ? "PASS" : "FAIL")};Line:{response.Line}";
    }
}
