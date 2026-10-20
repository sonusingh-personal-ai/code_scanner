namespace CodeScanner.Domain.Entities;

public class ResponseSummary
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public string? Parameters { get; set; }
    public string? Display { get; set; } // Maps to legacy column 'Dispaly'
    public string? Actual { get; set; }
    public string? Status { get; set; }
    public bool IsFinal { get; set; }

    public Response? Response { get; set; }
}
