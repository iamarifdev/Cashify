using FluentValidation;

namespace Cashify.Api.Features.Businesses.AddMember;

public class AddMemberValidator : AbstractValidator<AddMemberCommand>
{
    public AddMemberValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
    }
}
