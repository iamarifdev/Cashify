using Cashify.Api.Entities;

namespace Cashify.Api.Features.Businesses.AddMember;

public class AddMemberCommand
{
    public Guid UserId { get; set; }
    public Role Role { get; set; } = Role.Viewer;
}
