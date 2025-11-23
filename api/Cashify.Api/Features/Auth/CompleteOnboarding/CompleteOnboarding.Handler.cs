using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Auth.CompleteOnboarding;

public class CompleteOnboardingHandler
{
    private readonly AppDbContext _dbContext;
    private readonly Infrastructure.ActivityLogService _activityLogService;

    public CompleteOnboardingHandler(AppDbContext dbContext, Infrastructure.ActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<CompleteOnboardingResponse> Handle(Guid userId, CompleteOnboardingRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var fullName = request.FullName.Trim();
        if (!string.Equals(user.Name, fullName, StringComparison.Ordinal))
        {
            user.Name = fullName;
        }

        var existing = await _dbContext.BusinessMembers
            .Include(x => x.Business)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (existing?.Business != null)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new CompleteOnboardingResponse(existing.Business.Id, existing.Business.Name);
        }

        var defaultBusinessName = string.IsNullOrWhiteSpace(request.BusinessName)
            ? $"{fullName}'s Business"
            : request.BusinessName!.Trim();

        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = defaultBusinessName,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        var member = new BusinessMember
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            UserId = userId,
            Role = "owner"
        };

        _dbContext.Businesses.Add(business);
        _dbContext.BusinessMembers.Add(member);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _activityLogService.Log(userId, "business.create", business.Id, null, business.Id, new { business.Name }, cancellationToken);

        return new CompleteOnboardingResponse(business.Id, business.Name);
    }
}
