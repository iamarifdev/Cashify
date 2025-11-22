using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Lookups.CreatePaymentMethod;

public class CreatePaymentMethodHandler
{
    private readonly AppDbContext _dbContext;

    public CreatePaymentMethodHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> Handle(CreatePaymentMethodCommand command, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == command.BusinessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            BusinessId = command.BusinessId,
            Name = command.Name,
            CreatedByUserId = userId
        };

        _dbContext.PaymentMethods.Add(paymentMethod);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return paymentMethod.Id;
    }
}
