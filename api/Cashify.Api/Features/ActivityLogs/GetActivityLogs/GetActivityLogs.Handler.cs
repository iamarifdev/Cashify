using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.ActivityLogs.GetActivityLogs;

public class GetActivityLogsHandler
{
    private readonly AppDbContext _dbContext;

    public GetActivityLogsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivityLogItem>?> Handle(GetActivityLogsQuery query, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == query.BusinessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var logs = _dbContext.ActivityLogs
            .Where(x => x.BusinessId == query.BusinessId);

        if (query.CashbookId.HasValue)
        {
            logs = logs.Where(x => x.CashbookId == query.CashbookId);
        }

        query.Limit = Math.Clamp(query.Limit, 1, 200);
        query.Offset = Math.Max(query.Offset, 0);

        var items = await logs
            .OrderByDescending(x => x.OccurredAt)
            .Skip(query.Offset)
            .Take(query.Limit)
            .Select(x => new ActivityLogItem(x.Id, x.Action, x.UserId, x.BusinessId, x.CashbookId, x.EntityId, x.OccurredAt, x.MetadataJson))
            .ToListAsync(cancellationToken);

        return items;
    }
}

public record ActivityLogItem(Guid Id, string Action, Guid UserId, Guid? BusinessId, Guid? CashbookId, Guid? EntityId, DateTime OccurredAt, string? MetadataJson);
