using CodeScanner.Application.Common.Interfaces;
using CodeScanner.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ModelsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ModelDto>>> GetAll()
    {
        var list = await _context.Models
            .OrderBy(m => m.Position)
            .Select(m => new ModelDto
            {
                Id = m.Id,
                Name = m.Name,
                Value = m.Value,
                Position = m.Position
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{value}/setting")]
    public async Task<ActionResult> GetSetting(string value)
    {
        var setting = await _context.Settings
            .Include(s => s.SettingInfos)
            .FirstOrDefaultAsync(s => s.FileId == value);

        if (setting == null) return NotFound();

        return Ok(new
        {
            setting.Id,
            setting.FileId,
            setting.Header,
            setting.Footer,
            setting.Fields,
            SettingInfos = setting.SettingInfos.Select(i => new { i.Id, i.Parameters, i.Status }).ToList()
        });
    }
}
