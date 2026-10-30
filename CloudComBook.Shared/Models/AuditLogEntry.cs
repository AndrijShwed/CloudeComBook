// AuditLogEntry.cs
namespace CloudComBook.Shared.Models;

public class AuditLogEntry
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UserId { get; set; }
    public string? UserLogin { get; set; }
    public int? CommunityId { get; set; }
    public string Entity { get; set; } = "";
    public int? EntityId { get; set; }
    public string Action { get; set; } = "";
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? CommunityName { get; set; }
}
