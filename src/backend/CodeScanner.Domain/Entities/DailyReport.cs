namespace CodeScanner.Domain.Entities;

public class DailyReport
{
    public int Id { get; set; }
    public int? Date { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? FilePath { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}
