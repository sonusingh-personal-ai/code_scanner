namespace CodeScanner.Application.DTOs;

public class SaveResponseRequestDto
{
    public string Barcode { get; set; } = string.Empty;
    public int? QcStatus { get; set; } = 1; // 1: Testing, 2: Qc, 3: Oqc
    public int? VisualBy { get; set; }
    public int? TestedBy { get; set; }
    public int? ProductionLine { get; set; } = 1; // 1: Card, 2: Assembly
    public int? ProcessEngg { get; set; }
    public string? SerialCardNo { get; set; }
    public string? Model { get; set; }
    public string? ConProgNo { get; set; }
    public string? DisProgNo { get; set; }
    public string? SystemRating { get; set; }
    public int? PrinterModelId { get; set; }
    public int? Line { get; set; }
    public string? RawTelemetry { get; set; }
}

public class TelemetryParamDto
{
    public string Parameter { get; set; } = string.Empty;
    public string Display { get; set; } = "--";
    public string Actual { get; set; } = "--";
    public string Status { get; set; } = "N/A"; // OK, FAIL, N/A
    public bool IsPass => string.Equals(Status, "OK", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(Status, "PASS", StringComparison.OrdinalIgnoreCase);
}

public class TestResultDto
{
    public int ResponseId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public bool IsOverallPass { get; set; }
    public string Header { get; set; } = string.Empty;
    public string Footer { get; set; } = string.Empty;
    public string DisplayProgNo { get; set; } = string.Empty;
    public string ControlProgNo { get; set; } = string.Empty;
    public string SystemRating { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public List<TelemetryParamDto> Parameters { get; set; } = new();
    public string FormattedQrCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class PortConnectionRequestDto
{
    public string PortName { get; set; } = string.Empty;
    public int BaudRate { get; set; } = 9600;
}
