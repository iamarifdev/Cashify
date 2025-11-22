using FluentValidation;

namespace Cashify.Api.Features.Transactions.UpdateTransaction;

public class UpdateTransactionValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.Amount).NotEqual(0);
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.TransactionDate).NotEmpty();
    }
}

