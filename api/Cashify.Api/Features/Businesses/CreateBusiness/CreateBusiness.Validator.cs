using FluentValidation;

namespace Cashify.Api.Features.Businesses.CreateBusiness;

public class CreateBusinessValidator : AbstractValidator<CreateBusinessCommand>
{
    public CreateBusinessValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

