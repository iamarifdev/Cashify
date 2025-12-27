namespace Cashify.Api.Entities;

public class Business
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? CreatedByUser { get; set; }
    public ICollection<BusinessMember> Members { get; set; } = new List<BusinessMember>();
    public ICollection<Cashbook> Cashbooks { get; set; } = new List<Cashbook>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}

