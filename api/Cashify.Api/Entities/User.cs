namespace Cashify.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string GoogleUserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? PhotoUrl { get; set; }
    public bool EmailVerified { get; set; }
    public bool HasCompletedOnboarding { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    public ICollection<BusinessMember> Businesses { get; set; } = [];
    public ICollection<CashbookMember> Cashbooks { get; set; } = [];
}

