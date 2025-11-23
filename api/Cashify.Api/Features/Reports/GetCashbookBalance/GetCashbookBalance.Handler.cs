using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Reports.GetCashbookBalance;

public class GetCashbookBalanceHandler
{
    private readonly AppDbContext _dbContext;

    public GetCashbookBalanceHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal?> Handle(Guid businessId, Guid cashbookId, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.CashbookMembers.AnyAsync(x => x.CashbookId == cashbookId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var income = await _dbContext.Transactions
            .Where(x => x.BusinessId == businessId && x.CashbookId == cashbookId && x.Type == "income")
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var expense = await _dbContext.Transactions
            .Where(x => x.BusinessId == businessId && x.CashbookId == cashbookId && x.Type == "expense")
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var transfer = await _dbContext.Transactions
            .Where(x => x.BusinessId == businessId && x.CashbookId == cashbookId && x.Type == "transfer")
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        return income - expense + transfer;
    }
}
