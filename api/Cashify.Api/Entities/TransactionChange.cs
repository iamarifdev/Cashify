namespace Cashify.Api.Entities;

public class TransactionChange
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangeType { get; set; } = "updated";
    public string ChangesJson { get; set; } = "{}";

    public Transaction? Transaction { get; set; }
    public User? ChangedByUser { get; set; }
}

