using CodeScanner.Application.DTOs;

namespace CodeScanner.Application.Services;

public interface ILineService
{
    Task<List<LineDto>> GetAllLinesAsync();
    Task<LineDto> CreateLineAsync(string name, int? sequence = null);
    Task<bool> UpdateLineAsync(int id, string name, int? sequence = null);
    Task<bool> MoveLineAsync(int id, string direction);
    Task<bool> DeleteLineAsync(int id);
}
