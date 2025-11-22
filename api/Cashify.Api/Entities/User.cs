namespace Cashify.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string GoogleUserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    public ICollection<BusinessMember> Businesses { get; set; } = new List<BusinessMember>();
    public ICollection<CashbookMember> Cashbooks { get; set; } = new List<CashbookMember>();
}

