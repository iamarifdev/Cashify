using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Cashbooks.AddMember;

public class AddCashbookMemberHandler
{
    private readonly AppDbContext _dbContext;
    private readonly Infrastructure.ActivityLogService _activityLogService;

    public AddCashbookMemberHandler(AppDbContext dbContext, Infrastructure.ActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<bool> Handle(Guid businessId, Guid cashbookId, Guid actingUserId, AddCashbookMemberCommand command, CancellationToken cancellationToken)
    {
        var isBusinessOwner = await _dbContext.BusinessMembers
            .AnyAsync(x => x.BusinessId == businessId && x.UserId == actingUserId && x.Role == Role.Owner, cancellationToken);

        var isCashbookOwner = await _dbContext.CashbookMembers
            .AnyAsync(x => x.CashbookId == cashbookId && x.UserId == actingUserId && x.Role == Role.Owner, cancellationToken);

        if (!isBusinessOwner && !isCashbookOwner)
        {
            return false;
        }

        var exists = await _dbContext.CashbookMembers.AnyAsync(
            x => x.CashbookId == cashbookId && x.UserId == command.UserId,
            cancellationToken);

        if (exists)
        {
            return true;
        }

        _dbContext.CashbookMembers.Add(new CashbookMember
        {
            Id = Guid.NewGuid(),
            CashbookId = cashbookId,
            UserId = command.UserId,
            Role = command.Role
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _activityLogService.Log(actingUserId, "cashbook.member.add", businessId, cashbookId, command.UserId, new { command.Role }, cancellationToken);
        return true;
    }
}
