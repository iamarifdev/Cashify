namespace Cashify.Api.Entities;

public class CashbookMember
{
    public Guid Id { get; set; }
    public Guid CashbookId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "member";

    public Cashbook? Cashbook { get; set; }
    public User? User { get; set; }
}

