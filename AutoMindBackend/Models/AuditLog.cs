public class AuditLog
{
    public int Id { get; set; }

    public string? UserId { get; set; }
    public string? UserName { get; set; }

    public string Action { get; set; } = null!; 
    public string EntityName { get; set; } = null!;

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
}