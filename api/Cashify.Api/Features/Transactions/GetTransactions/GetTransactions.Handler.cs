using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Transactions.GetTransactions;

public class GetTransactionsHandler
{
    private readonly AppDbContext _dbContext;

    public GetTransactionsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TransactionListItem>> Handle(Guid businessId, Guid cashbookId, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.CashbookMembers.AnyAsync(x => x.CashbookId == cashbookId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return Array.Empty<TransactionListItem>();
        }

        return await _dbContext.Transactions
            .Where(x => x.BusinessId == businessId && x.CashbookId == cashbookId)
            .OrderByDescending(x => x.TransactionDate)
            .Select(x => new TransactionListItem(
                x.Id,
                x.Amount,
                x.Type,
                x.CategoryId,
                x.ContactId,
                x.PaymentMethodId,
                x.Description,
                x.TransactionDate,
                x.Version))
            .ToListAsync(cancellationToken);
    }
}

public record TransactionListItem(
    Guid Id,
    decimal Amount,
    string Type,
    Guid? CategoryId,
    Guid? ContactId,
    Guid? PaymentMethodId,
    string? Description,
    DateTime TransactionDate,
    int Version);
