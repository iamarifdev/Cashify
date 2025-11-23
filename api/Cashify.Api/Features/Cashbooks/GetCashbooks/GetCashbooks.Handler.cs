using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Cashbooks.GetCashbooks;

public class GetCashbooksHandler
{
    private readonly AppDbContext _dbContext;

    public GetCashbooksHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CashbookListResult> Handle(Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == businessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return new CashbookListResult(false, Array.Empty<CashbookSummary>());
        }

        var cashbooks = await _dbContext.Cashbooks
            .Where(x => x.BusinessId == businessId)
            .Select(x => new CashbookSummary(x.Id, x.Name, x.Currency))
            .ToListAsync(cancellationToken);
        return new CashbookListResult(true, cashbooks);
    }
}

public record CashbookSummary(Guid Id, string Name, string Currency);
public record CashbookListResult(bool IsMember, IReadOnlyList<CashbookSummary> Cashbooks);
