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

    public async Task<IReadOnlyList<CashbookSummary>> Handle(Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == businessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return Array.Empty<CashbookSummary>();
        }

        return await _dbContext.Cashbooks
            .Where(x => x.BusinessId == businessId)
            .Select(x => new CashbookSummary(x.Id, x.Name, x.Currency))
            .ToListAsync(cancellationToken);
    }
}

public record CashbookSummary(Guid Id, string Name, string Currency);
