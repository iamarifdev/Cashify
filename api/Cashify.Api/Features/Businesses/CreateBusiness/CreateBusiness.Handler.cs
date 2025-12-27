using Cashify.Api.Database;
using Cashify.Api.Entities;

namespace Cashify.Api.Features.Businesses.CreateBusiness;

public class CreateBusinessHandler
{
    private readonly AppDbContext _dbContext;
    private readonly Infrastructure.ActivityLogService _activityLogService;

    public CreateBusinessHandler(AppDbContext dbContext, Infrastructure.ActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Guid> Handle(CreateBusinessCommand command, Guid userId, CancellationToken cancellationToken)
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        var member = new BusinessMember
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            UserId = userId,
            Role = Role.Owner
        };

        _dbContext.Businesses.Add(business);
        _dbContext.BusinessMembers.Add(member);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _activityLogService.Log(userId, "business.create", business.Id, null, business.Id, new { command.Name }, cancellationToken);
        return business.Id;
    }
}
