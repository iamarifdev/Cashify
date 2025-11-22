using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Transactions.GetTransactionChanges;

public class GetTransactionChangesHandler
{
    private readonly AppDbContext _dbContext;

    public GetTransactionChangesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TransactionChangeDto>> Handle(Guid businessId, Guid cashbookId, Guid transactionId, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.CashbookMembers.AnyAsync(x => x.CashbookId == cashbookId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return Array.Empty<TransactionChangeDto>();
        }

        return await _dbContext.TransactionChanges
            .Where(x => x.TransactionId == transactionId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => new TransactionChangeDto(x.Id, x.ChangeType, x.ChangesJson, x.ChangedAt, x.ChangedByUserId))
            .ToListAsync(cancellationToken);
    }
}

public record TransactionChangeDto(Guid Id, string ChangeType, string ChangesJson, DateTime ChangedAt, Guid ChangedByUserId);
