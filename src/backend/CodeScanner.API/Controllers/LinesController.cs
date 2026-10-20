using CodeScanner.Application.DTOs;
using CodeScanner.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LinesController : ControllerBase
{
    private readonly ILineService _lineService;

    public LinesController(ILineService lineService)
    {
        _lineService = lineService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LineDto>>> GetAll()
    {
        var lines = await _lineService.GetAllLinesAsync();
        return Ok(lines);
    }

    public record CreateLineRequest(string Name, int? Sequence);
    public record UpdateLineRequest(string Name, int? Sequence);
    public record MoveLineRequest(string Direction);

    [HttpPost]
    public async Task<ActionResult<LineDto>> Create([FromBody] CreateLineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Line name cannot be empty");
        }
        var created = await _lineService.CreateLineAsync(request.Name, request.Sequence);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateLineRequest request)
    {
        bool success = await _lineService.UpdateLineAsync(id, request.Name, request.Sequence);
        if (!success) return NotFound();
        return Ok(new { success = true });
    }

    [HttpPost("{id:int}/move")]
    public async Task<ActionResult> Move(int id, [FromBody] MoveLineRequest request)
    {
        bool success = await _lineService.MoveLineAsync(id, request.Direction);
        return Ok(new { success });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        bool success = await _lineService.DeleteLineAsync(id);
        if (!success) return NotFound();
        return Ok(new { success = true });
    }
}
