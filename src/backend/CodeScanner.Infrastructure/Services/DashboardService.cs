using System.Drawing.Printing;
using CodeScanner.Application.Common.Interfaces;
using CodeScanner.Application.DTOs;
using CodeScanner.Application.Services;
using CodeScanner.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeScanner.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly ISerialPortService _serialPortService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IApplicationDbContext context,
        ISerialPortService serialPortService,
        ILogger<DashboardService> logger)
    {
        _context = context;
        _serialPortService = serialPortService;
        _logger = logger;
    }

    public async Task<DashboardInitDto> GetInitialDataAsync()
    {
        var init = new DashboardInitDto();

        try
        {
            // 1. Models
            init.Models = await _context.Models
                .OrderBy(m => m.Position)
                .Select(m => new ModelDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Value = m.Value,
                    Position = m.Position
                })
                .ToListAsync();

            // 2. Office Members
            var allMembers = await _context.OfficeMembers
                .OrderBy(m => m.Name)
                .Select(m => new OfficeMemberDto
                {
                    Id = m.Id,
                    Type = m.Type,
                    Name = m.Name
                })
                .ToListAsync();

            init.VisualByList = allMembers.Where(m => m.Type == OfficeMemberType.VisualBy).ToList();
            init.TestedByList = allMembers.Where(m => m.Type == OfficeMemberType.TestedBy).ToList();
            init.ProcessEngList = allMembers.Where(m => m.Type == OfficeMemberType.ProcEng).ToList();
            init.ProgDisplayNoList = allMembers.Where(m => m.Type == OfficeMemberType.ProgDisNo).ToList();

            // 3. Lines
            init.Lines = await _context.Lines
                .OrderBy(l => l.Sequence)
                .Select(l => new LineDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Sequence = l.Sequence
                })
                .ToListAsync();

            // 4. Printer Models
            init.PrinterModels = await _context.PrinterModels
                .Include(p => p.ModelValues)
                .OrderBy(p => p.Name)
                .Select(p => new PrinterModelDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ModelValues = p.ModelValues.Select(v => new PrinterModelValueDto
                    {
                        Id = v.Id,
                        ModelId = v.ModelId,
                        Name = v.Name,
                        Value = v.Value
                    }).ToList()
                })
                .ToListAsync();

            // 5. Available Serial Ports
            init.AvailablePorts = _serialPortService.GetAvailablePorts();

            // 6. Available System Printers
            init.AvailablePrinters = GetInstalledPrinters();

            // 7. Today's Shift Stats
            init.ShiftStats = await GetShiftStatsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assembling initial dashboard data");
        }

        return init;
    }

    public async Task<ShiftStatsDto> GetShiftStatsAsync(int? lineId = null)
    {
        var stats = new ShiftStatsDto();
        try
        {
            var today = DateTime.Today;
            var todayStr = today.ToString("dd-MM-yyyy");
            var altTodayStr = today.ToString("yyyy-MM-dd");

            var query = _context.Responses.AsNoTracking()
                .Where(r => r.CreatedOn >= today || r.CurrentDate == todayStr || r.CurrentDate == altTodayStr);

            if (lineId.HasValue && lineId.Value > 0)
            {
                query = query.Where(r => r.Line == lineId.Value);
            }

            var counts = await query
                .GroupBy(r => r.QcStatus == 1) // 1: Pass
                .Select(g => new { IsPass = g.Key, Count = g.Count() })
                .ToListAsync();

            stats.PassCount = counts.FirstOrDefault(c => c.IsPass)?.Count ?? 0;
            stats.FailCount = counts.FirstOrDefault(c => !c.IsPass)?.Count ?? 0;
            stats.TotalTested = stats.PassCount + stats.FailCount;
            stats.CurrentShift = DetermineShift(DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing shift metrics");
        }
        return stats;
    }

    private static string DetermineShift(DateTime time)
    {
        var hour = time.Hour;
        if (hour >= 6 && hour < 14) return "Shift A (Morning)";
        if (hour >= 14 && hour < 22) return "Shift B (Evening)";
        return "Shift C (Night)";
    }

    private List<string> GetInstalledPrinters()
    {
        var list = new List<string> { "SIMULATOR_PRINTER" };
        try
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                // Filter out PDF/XPS virtual printers if desired, or include them
                list.Add(printer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unable to enumerate system printers: {Message}", ex.Message);
        }
        return list;
    }
}
