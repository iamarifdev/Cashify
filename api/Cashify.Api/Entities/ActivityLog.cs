namespace Cashify.Api.Entities;

public class ActivityLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = default!;
    public Guid? BusinessId { get; set; }
    public Guid? CashbookId { get; set; }
    public Guid? EntityId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? MetadataJson { get; set; }

    public User? User { get; set; }
}

