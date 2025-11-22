using Cashify.Api.Database;
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

        return await _dbContext.Businesses
            .Where(x => x.Id == businessId)
            .Select(x => new BusinessDetails(x.Id, x.Name, x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public record BusinessDetails(Guid Id, string Name, DateTime CreatedAt);
