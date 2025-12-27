using FluentValidation;

namespace Cashify.Api.Features.Cashbooks.AddMember;

public class AddCashbookMemberValidator : AbstractValidator<AddCashbookMemberCommand>
{
    public AddCashbookMemberValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum().WithMessage("Role must be Owner, Editor, or Viewer");
    }
}
