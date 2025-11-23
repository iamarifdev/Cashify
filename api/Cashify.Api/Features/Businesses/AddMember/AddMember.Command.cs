namespace Cashify.Api.Features.Businesses.AddMember;

public class AddMemberCommand
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "member";
}
