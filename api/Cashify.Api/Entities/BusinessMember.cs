namespace Cashify.Api.Entities;

public class BusinessMember
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid UserId { get; set; }
    public Role Role { get; set; } = Role.Viewer;

    public Business? Business { get; set; }
    public User? User { get; set; }
}

