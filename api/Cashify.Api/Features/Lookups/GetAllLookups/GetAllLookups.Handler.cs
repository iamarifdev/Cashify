using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Lookups.GetAllLookups;

public class GetAllLookupsHandler
{
    private readonly AppDbContext _dbContext;

    public GetAllLookupsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object> Handle(Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == businessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return Array.Empty<object>();
        }

        var categories = await _dbContext.Categories.Where(x => x.BusinessId == businessId).ToListAsync(cancellationToken);
        var contacts = await _dbContext.Contacts.Where(x => x.BusinessId == businessId).ToListAsync(cancellationToken);
        var paymentMethods = await _dbContext.PaymentMethods.Where(x => x.BusinessId == businessId).ToListAsync(cancellationToken);

        return new
        {
            categories,
            contacts,
            paymentMethods
        };
    }
}
