using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Cashbooks.CreateCashbook;

public class CreateCashbookHandler
{
    private readonly AppDbContext _dbContext;

    public CreateCashbookHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> Handle(Guid businessId, Guid userId, CreateCashbookCommand command, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == businessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var cashbook = new Cashbook
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = command.Name,
            Currency = command.Currency,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Cashbooks.Add(cashbook);
        _dbContext.CashbookMembers.Add(new CashbookMember
        {
            Id = Guid.NewGuid(),
            CashbookId = cashbook.Id,
            UserId = userId,
            Role = "owner"
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return cashbook.Id;
    }
}
