namespace Cashify.Api.Entities;

public class Contact
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = default!;
    public string Type { get; set; } = "customer"; // customer | supplier
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business? Business { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

