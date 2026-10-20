using CodeScanner.Application.DTOs;
using CodeScanner.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("init")]
    public async Task<ActionResult<DashboardInitDto>> GetInitData()
    {
        var data = await _dashboardService.GetInitialDataAsync();
        return Ok(data);
    }

    [HttpGet("shift-stats")]
    public async Task<ActionResult<ShiftStatsDto>> GetShiftStats([FromQuery] int? lineId)
    {
        var stats = await _dashboardService.GetShiftStatsAsync(lineId);
        return Ok(stats);
    }
}
