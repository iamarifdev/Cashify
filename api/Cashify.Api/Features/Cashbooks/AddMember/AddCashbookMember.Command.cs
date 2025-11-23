namespace Cashify.Api.Features.Cashbooks.AddMember;

public class AddCashbookMemberCommand
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "member";
}
