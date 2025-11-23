using System.Text;
using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Reports.ExportTransactions;

public class ExportTransactionsHandler
{
    private readonly AppDbContext _dbContext;

    public ExportTransactionsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(string ContentType, byte[] Data)?> Handle(ExportTransactionsQuery query, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.CashbookMembers.AnyAsync(x => x.CashbookId == query.CashbookId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var items = await _dbContext.Transactions
            .Where(x => x.BusinessId == query.BusinessId && x.CashbookId == query.CashbookId)
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync(cancellationToken);

        if (query.Format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,Amount,Type,CategoryId,ContactId,PaymentMethodId,Description,TransactionDate,Version");
            foreach (var t in items)
            {
                sb.AppendLine($"{t.Id},{t.Amount},{t.Type},{t.CategoryId},{t.ContactId},{t.PaymentMethodId},\"{t.Description}\",{t.TransactionDate:o},{t.Version}");
            }

            return ("text/csv", Encoding.UTF8.GetBytes(sb.ToString()));
        }

        // Placeholder for Excel or other formats
        return ("application/octet-stream", Array.Empty<byte>());
    }
}
