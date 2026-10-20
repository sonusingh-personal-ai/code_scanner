namespace CodeScanner.Domain.Entities;

public class Line
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; }
}
