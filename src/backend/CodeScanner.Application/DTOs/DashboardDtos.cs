using CodeScanner.Domain.Enums;

namespace CodeScanner.Application.DTOs;

public class DashboardInitDto
{
    public List<ModelDto> Models { get; set; } = new();
    public List<OfficeMemberDto> VisualByList { get; set; } = new();
    public List<OfficeMemberDto> TestedByList { get; set; } = new();
    public List<OfficeMemberDto> ProcessEngList { get; set; } = new();
    public List<OfficeMemberDto> ProgDisplayNoList { get; set; } = new();
    public List<LineDto> Lines { get; set; } = new();
    public List<PrinterModelDto> PrinterModels { get; set; } = new();
    public List<string> AvailablePorts { get; set; } = new();
    public List<string> AvailablePrinters { get; set; } = new();
    public ShiftStatsDto ShiftStats { get; set; } = new();
}

public class ModelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Position { get; set; }
}

public class OfficeMemberDto
{
    public int Id { get; set; }
    public OfficeMemberType Type { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class LineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
}

public class PrinterModelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PrinterModelValueDto> ModelValues { get; set; } = new();
}

public class PrinterModelValueDto
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ShiftStatsDto
{
    public int TotalTested { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public double YieldPercentage => TotalTested > 0 ? Math.Round((double)PassCount / TotalTested * 100.0, 1) : 0.0;
    public string CurrentShift { get; set; } = "General";
    public string Date { get; set; } = DateTime.Now.ToString("dd-MM-yyyy");
}
