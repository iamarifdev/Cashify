namespace Cashify.Api.Entities;

public class Cashbook
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = default!;
    public string Currency { get; set; } = "USD";
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business? Business { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<CashbookMember> Members { get; set; } = new List<CashbookMember>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

