namespace CodeScanner.Domain.Entities;

public class SettingInfo
{
    public int Id { get; set; }
    public int SettingId { get; set; }
    public string Parameters { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? CreatedOn { get; set; } = DateTime.Now;

    public Setting? Setting { get; set; }
}
