using CodeScanner.Application.DTOs;

namespace CodeScanner.Application.Services;

public interface IDashboardService
{
    Task<DashboardInitDto> GetInitialDataAsync();
    Task<ShiftStatsDto> GetShiftStatsAsync(int? lineId = null);
}
