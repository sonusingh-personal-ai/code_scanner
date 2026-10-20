namespace CodeScanner.Domain.Entities;

public class Setting
{
    public int Id { get; set; }
    public string Header { get; set; } = string.Empty;
    public string Footer { get; set; } = string.Empty;
    public int Fields { get; set; }
    public string FileId { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; }

    public ICollection<SettingInfo> SettingInfos { get; set; } = new List<SettingInfo>();
}
