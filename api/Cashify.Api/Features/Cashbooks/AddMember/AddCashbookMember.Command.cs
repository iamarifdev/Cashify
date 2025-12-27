using Cashify.Api.Entities;

namespace Cashify.Api.Features.Cashbooks.AddMember;

public class AddCashbookMemberCommand
{
    public Guid UserId { get; set; }
    public Role Role { get; set; } = Role.Viewer;
}
