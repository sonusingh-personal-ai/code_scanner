using CodeScanner.Domain.Enums;

namespace CodeScanner.Domain.Entities;

public class OfficeMember
{
    public int Id { get; set; }
    public OfficeMemberType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime InsertedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; }
}
