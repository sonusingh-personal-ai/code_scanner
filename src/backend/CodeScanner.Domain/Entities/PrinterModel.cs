namespace CodeScanner.Domain.Entities;

public class PrinterModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; }

    public ICollection<PrinterModelValue> ModelValues { get; set; } = new List<PrinterModelValue>();
}
