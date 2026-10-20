using CodeScanner.Application.Common.Interfaces;
using CodeScanner.Application.DTOs;
using CodeScanner.Application.Services;
using CodeScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeScanner.Infrastructure.Services;

public class LineService : ILineService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LineService> _logger;

    public LineService(IApplicationDbContext context, ILogger<LineService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<LineDto>> GetAllLinesAsync()
    {
        return await _context.Lines
            .OrderBy(l => l.Sequence)
            .ThenBy(l => l.Id)
            .Select(l => new LineDto
            {
                Id = l.Id,
                Name = l.Name,
                Sequence = l.Sequence
            })
            .ToListAsync();
    }

    public async Task<LineDto> CreateLineAsync(string name, int? sequence = null)
    {
        int finalSeq = sequence ?? 0;
        if (finalSeq <= 0)
        {
            int maxSeq = await _context.Lines.MaxAsync(l => (int?)l.Sequence) ?? 0;
            finalSeq = maxSeq + 1;
        }

        var line = new Line
        {
            Name = name.Trim(),
            Sequence = finalSeq,
            CreatedOn = DateTime.Now
        };

        _context.Lines.Add(line);
        await _context.SaveChangesAsync();

        return new LineDto
        {
            Id = line.Id,
            Name = line.Name,
            Sequence = line.Sequence
        };
    }

    public async Task<bool> UpdateLineAsync(int id, string name, int? sequence = null)
    {
        var line = await _context.Lines.FindAsync(id);
        if (line == null) return false;

        line.Name = name.Trim();
        if (sequence.HasValue && sequence.Value > 0)
        {
            line.Sequence = sequence.Value;
        }
        line.ModifiedOn = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoveLineAsync(int id, string direction)
    {
        var allLines = await _context.Lines.OrderBy(l => l.Sequence).ToListAsync();
        int currentIndex = allLines.FindIndex(l => l.Id == id);
        if (currentIndex < 0) return false;

        int targetIndex = string.Equals(direction, "up", StringComparison.OrdinalIgnoreCase)
            ? currentIndex - 1
            : currentIndex + 1;

        if (targetIndex < 0 || targetIndex >= allLines.Count) return false;

        var currentLine = allLines[currentIndex];
        var targetLine = allLines[targetIndex];

        // Swap sequences
        int tempSeq = currentLine.Sequence;
        currentLine.Sequence = targetLine.Sequence;
        targetLine.Sequence = tempSeq;

        // If sequences were identical, normalize
        if (currentLine.Sequence == targetLine.Sequence)
        {
            for (int i = 0; i < allLines.Count; i++)
            {
                allLines[i].Sequence = i + 1;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteLineAsync(int id)
    {
        var line = await _context.Lines.FindAsync(id);
        if (line == null) return false;

        // Nullify foreign references in Response
        var responsesWithLine = await _context.Responses.Where(r => r.Line == id).ToListAsync();
        foreach (var r in responsesWithLine)
        {
            r.Line = null;
        }

        _context.Lines.Remove(line);
        await _context.SaveChangesAsync();
        return true;
    }
}
