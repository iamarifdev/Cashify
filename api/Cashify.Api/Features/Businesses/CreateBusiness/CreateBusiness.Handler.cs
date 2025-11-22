using Cashify.Api.Database;
using Cashify.Api.Entities;

namespace Cashify.Api.Features.Businesses.CreateBusiness;

public class CreateBusinessHandler
{
    private readonly AppDbContext _dbContext;

    public CreateBusinessHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
            Role = "owner"
        };

        _dbContext.Businesses.Add(business);
        _dbContext.BusinessMembers.Add(member);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return business.Id;
    }
}
