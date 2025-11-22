using FluentValidation;

namespace Cashify.Api.Features.Cashbooks.CreateCashbook;

public class CreateCashbookValidator : AbstractValidator<CreateCashbookCommand>
{
    public CreateCashbookValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
    }
}

