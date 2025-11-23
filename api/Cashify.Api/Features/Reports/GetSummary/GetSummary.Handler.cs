using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Reports.GetSummary;

public class GetSummaryHandler
{
    private readonly AppDbContext _dbContext;

    public GetSummaryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SummaryResult?> Handle(GetSummaryQuery query, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == query.BusinessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var tx = _dbContext.Transactions
            .Where(x => x.BusinessId == query.BusinessId);

        if (query.CashbookId.HasValue)
        {
            var isCashbookMember = await _dbContext.CashbookMembers.AnyAsync(x => x.CashbookId == query.CashbookId && x.UserId == userId, cancellationToken);
            if (!isCashbookMember)
            {
                return null;
            }

            tx = tx.Where(x => x.CashbookId == query.CashbookId);
        }

        tx = GetSummaryRangeFilter.ApplyRangeFilter(tx, query);

        var income = await tx.Where(x => x.Type == "income").SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        var expense = await tx.Where(x => x.Type == "expense").SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        var transfer = await tx.Where(x => x.Type == "transfer").SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        return new SummaryResult(income, expense, transfer);
    }
}

public record SummaryResult(decimal Income, decimal Expense, decimal Transfer);

internal static class GetSummaryRangeFilter
{
    public static IQueryable<Entities.Transaction> ApplyRangeFilter(IQueryable<Entities.Transaction> query, GetSummaryQuery request)
    {
        var offset = TimeSpan.Zero; // UTC enforced

        // Explicit date range overrides presets (dates are assumed UTC)
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            var startLocal = request.StartDate.Value.Date;
            var endLocal = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            var startUtc = new DateTimeOffset(startLocal, offset).UtcDateTime;
            var endUtc = new DateTimeOffset(endLocal, offset).UtcDateTime;

            return query.Where(x => x.TransactionDate >= startUtc && x.TransactionDate <= endUtc);
        }

        var nowLocal = DateTimeOffset.UtcNow.ToOffset(offset);
        return request.Range?.ToLowerInvariant() switch
        {
            "daily" => query.Where(x => x.TransactionDate >= nowLocal.Date && x.TransactionDate < nowLocal.Date.AddDays(1)),
            "weekly" => query.Where(x => x.TransactionDate >= nowLocal.Date.AddDays(-6) && x.TransactionDate < nowLocal.Date.AddDays(1)),
            "monthly" => query.Where(x => x.TransactionDate >= new DateTimeOffset(nowLocal.Year, nowLocal.Month, 1, 0, 0, 0, offset).UtcDateTime),
            _ => query
        };
    }
}
