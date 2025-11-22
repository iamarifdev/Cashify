namespace Cashify.Api.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid CashbookId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = "expense"; // income | expense | transfer
    public Guid? CategoryId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }
    public Guid UpdatedByUserId { get; set; }
    public int Version { get; set; } = 1;

    public Business? Business { get; set; }
    public Cashbook? Cashbook { get; set; }
    public Category? Category { get; set; }
    public Contact? Contact { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
    public ICollection<TransactionChange> Changes { get; set; } = new List<TransactionChange>();
}

