namespace CodeScanner.Domain.Entities;

public class PrinterModelValue
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; }

    public PrinterModel? PrinterModel { get; set; }
}
