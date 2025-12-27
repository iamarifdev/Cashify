using FluentValidation;

namespace Cashify.Api.Features.Businesses.AddMember;

public class AddMemberValidator : AbstractValidator<AddMemberCommand>
{
    public AddMemberValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum().WithMessage("Role must be Owner, Editor, or Viewer");
    }
}
