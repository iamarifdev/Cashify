using FluentValidation;

namespace Cashify.Api.Features.Cashbooks.AddMember;

public class AddCashbookMemberValidator : AbstractValidator<AddCashbookMemberCommand>
{
    public AddCashbookMemberValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
    }
}
