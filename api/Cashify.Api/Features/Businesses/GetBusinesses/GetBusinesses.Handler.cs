using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Businesses.GetBusinesses;

public class GetBusinessesHandler
{
    private readonly AppDbContext _dbContext;

    public GetBusinessesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<BusinessSummary>> Handle(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.BusinessMembers
            .Where(x => x.UserId == userId)
            .Select(x => new BusinessSummary(x.BusinessId, x.Business!.Name, x.Role.ToString()))
            .ToListAsync(cancellationToken);
    }
}

public record BusinessSummary(Guid Id, string Name, string Role);
