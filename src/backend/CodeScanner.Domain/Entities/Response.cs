using CodeScanner.Domain.Enums;

namespace CodeScanner.Domain.Entities;

public class Response
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public int? QcStatus { get; set; }
    public int? VisualBy { get; set; }
    public int? TestedBy { get; set; }
    public int? ProductionLine { get; set; }
    public int? ProcessEngg { get; set; }
    public string? SerialCardNo { get; set; }
    public string? Model { get; set; }
    public string? ConProgNo { get; set; }
    public string? DisProgNo { get; set; }
    public string? SystemRating { get; set; }
    public string? CurrentDate { get; set; }
    public string? CurrentTime { get; set; }
    public DateTime? ResponseTime { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public int? PrinterModelId { get; set; }
    public int? Line { get; set; }

    public ICollection<ResponseSummary> Summaries { get; set; } = new List<ResponseSummary>();
}
