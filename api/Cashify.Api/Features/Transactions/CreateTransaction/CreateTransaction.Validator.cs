using FluentValidation;

namespace Cashify.Api.Features.Transactions.CreateTransaction;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.Amount).NotEqual(0);
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.TransactionDate).NotEmpty();
    }
}

