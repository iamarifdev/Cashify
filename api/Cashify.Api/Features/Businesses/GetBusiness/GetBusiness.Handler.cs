using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Businesses.GetBusiness;

public class GetBusinessHandler
{
    private readonly AppDbContext _dbContext;

    public GetBusinessHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BusinessDetails?> Handle(Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == businessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var business = await _dbContext.Businesses
            .Where(x => x.Id == businessId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (business is null)
        {
            return null;
        }

        var members = await _dbContext.BusinessMembers
            .Where(x => x.BusinessId == businessId)
            .Join(_dbContext.Users,
                member => member.UserId,
                user => user.Id,
                (member, user) => new { member, user })
            .OrderBy(x => x.member.Role)
            .Select(x => new BusinessMemberResponse(
                x.user.Id,
                x.user.Email,
                x.user.Name,
                x.member.Role.ToString(),
                x.member.Id
            ))
            .ToListAsync(cancellationToken);

        return new BusinessDetails(
            business.Id,
            business.Name,
            business.Description,
            business.CreatedAt,
            business.UpdatedAt,
            members
        );
    }
}

public record BusinessDetails(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<BusinessMemberResponse> Members
);

public record BusinessMemberResponse(
    Guid UserId,
    string Email,
    string Name,
    string Role,
    Guid JoinedAt
);
