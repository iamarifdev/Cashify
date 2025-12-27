using Cashify.Api.Database;
using Cashify.Api.Entities;

namespace Cashify.Api.Infrastructure;

public class ActivityLogService
{
    private readonly AppDbContext _dbContext;

    public ActivityLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Log(Guid userId, string action, Guid? businessId = null, Guid? cashbookId = null, Guid? entityId = null, object? metadata = null, CancellationToken cancellationToken = default)
    {
        var entry = new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            BusinessId = businessId,
            CashbookId = cashbookId,
            EntityId = entityId,
            OccurredAt = DateTime.UtcNow,
            MetadataJson = metadata is null ? null : System.Text.Json.JsonSerializer.Serialize(metadata)
        };

        _dbContext.ActivityLogs.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
