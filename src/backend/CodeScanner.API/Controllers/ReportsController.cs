using CodeScanner.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ReportsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("responses")]
    public async Task<ActionResult> GetResponses(
        [FromQuery] string? search,
        [FromQuery] int? lineId,
        [FromQuery] int? qcStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Responses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Barcode.Contains(search) || (r.Model != null && r.Model.Contains(search)));
        }

        if (lineId.HasValue && lineId.Value > 0)
        {
            query = query.Where(r => r.Line == lineId.Value);
        }

        if (qcStatus.HasValue)
        {
            query = query.Where(r => r.QcStatus == qcStatus.Value);
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.Barcode,
                r.Model,
                r.QcStatus,
                IsPass = r.QcStatus == 1,
                r.CurrentDate,
                r.CurrentTime,
                r.Line,
                r.SerialCardNo,
                r.ConProgNo,
                r.DisProgNo,
                r.SystemRating
            })
            .ToListAsync();

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            items
        });
    }

    [HttpGet("responses/{id:int}")]
    public async Task<ActionResult> GetResponseDetails(int id)
    {
        var response = await _context.Responses
            .Include(r => r.Summaries)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (response == null) return NotFound();

        return Ok(new
        {
            response.Id,
            response.Barcode,
            response.Model,
            response.QcStatus,
            IsPass = response.QcStatus == 1,
            response.CurrentDate,
            response.CurrentTime,
            response.Line,
            response.SerialCardNo,
            response.ConProgNo,
            response.DisProgNo,
            response.SystemRating,
            Summaries = response.Summaries.Select(s => new
            {
                s.Id,
                s.Parameters,
                s.Display,
                s.Actual,
                s.Status,
                s.IsFinal
            }).ToList()
        });
    }
}
