using CodeScanner.Application.Common.Interfaces;
using CodeScanner.Application.DTOs;
using CodeScanner.Domain.Entities;
using CodeScanner.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OfficeMembersController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public OfficeMembersController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<OfficeMemberDto>>> GetAll([FromQuery] OfficeMemberType? type)
    {
        var query = _context.OfficeMembers.AsQueryable();
        if (type.HasValue)
        {
            query = query.Where(m => m.Type == type.Value);
        }

        var list = await query
            .OrderBy(m => m.Name)
            .Select(m => new OfficeMemberDto
            {
                Id = m.Id,
                Type = m.Type,
                Name = m.Name
            })
            .ToListAsync();

        return Ok(list);
    }

    public record CreateMemberRequest(string Name, OfficeMemberType Type);

    [HttpPost]
    public async Task<ActionResult<OfficeMemberDto>> Create([FromBody] CreateMemberRequest request)
    {
        var member = new OfficeMember
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            InsertedOn = DateTime.Now
        };
        _context.OfficeMembers.Add(member);
        await _context.SaveChangesAsync();

        return Ok(new OfficeMemberDto { Id = member.Id, Name = member.Name, Type = member.Type });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var member = await _context.OfficeMembers.FindAsync(id);
        if (member == null) return NotFound();

        _context.OfficeMembers.Remove(member);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
