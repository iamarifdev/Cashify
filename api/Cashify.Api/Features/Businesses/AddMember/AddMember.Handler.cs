using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Businesses.AddMember;

public class AddMemberHandler
{
    private readonly AppDbContext _dbContext;
    private readonly Infrastructure.ActivityLogService _activityLogService;

    public AddMemberHandler(AppDbContext dbContext, Infrastructure.ActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<bool> Handle(Guid businessId, Guid actingUserId, AddMemberCommand command, CancellationToken cancellationToken)
    {
        var isOwner = await _dbContext.BusinessMembers
            .AnyAsync(x => x.BusinessId == businessId && x.UserId == actingUserId && x.Role == Role.Owner, cancellationToken);

        if (!isOwner)
        {
            return false;
        }

        var exists = await _dbContext.BusinessMembers
            .AnyAsync(x => x.BusinessId == businessId && x.UserId == command.UserId, cancellationToken);

        if (exists)
        {
            return true;
        }

        _dbContext.BusinessMembers.Add(new BusinessMember
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            UserId = command.UserId,
            Role = command.Role
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _activityLogService.Log(actingUserId, "business.member.add", businessId, null, command.UserId, new { command.Role }, cancellationToken);
        return true;
    }
}
