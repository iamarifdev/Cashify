using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Transactions.CreateTransaction;

public class CreateTransactionHandler
{
    private readonly AppDbContext _dbContext;
    private readonly Infrastructure.ActivityLogService _activityLogService;

    public CreateTransactionHandler(AppDbContext dbContext, Infrastructure.ActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Guid?> Handle(Guid businessId, Guid cashbookId, Guid userId, CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.CashbookMembers.AnyAsync(x => x.CashbookId == cashbookId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        // Inline creation stubs; resolve IDs if provided
        if (command.CategoryId == null && !string.IsNullOrWhiteSpace(command.InlineCategoryName))
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = command.InlineCategoryName,
                Type = command.Type,
                CreatedByUserId = userId
            };
            _dbContext.Categories.Add(category);
            command.CategoryId = category.Id;
        }

        if (command.ContactId == null && !string.IsNullOrWhiteSpace(command.InlineContactName))
        {
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = command.InlineContactName,
                Type = "customer",
                CreatedByUserId = userId
            };
            _dbContext.Contacts.Add(contact);
            command.ContactId = contact.Id;
        }

        if (command.PaymentMethodId == null && !string.IsNullOrWhiteSpace(command.InlinePaymentMethodName))
        {
            var paymentMethod = new PaymentMethod
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = command.InlinePaymentMethodName,
                CreatedByUserId = userId
            };
            _dbContext.PaymentMethods.Add(paymentMethod);
            command.PaymentMethodId = paymentMethod.Id;
        }

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CashbookId = cashbookId,
            Amount = command.Amount,
            Type = command.Type,
            CategoryId = command.CategoryId,
            ContactId = command.ContactId,
            PaymentMethodId = command.PaymentMethodId,
            Description = command.Description,
            TransactionDate = command.TransactionDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = userId,
            UpdatedByUserId = userId,
            Version = 1
        };

        _dbContext.Transactions.Add(tx);
        _dbContext.TransactionChanges.Add(new TransactionChange
        {
            Id = Guid.NewGuid(),
            TransactionId = tx.Id,
            ChangedByUserId = userId,
            ChangeType = "created",
            ChangesJson = "{}"
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _activityLogService.Log(userId, "transaction.create", businessId, cashbookId, tx.Id, new { command.Amount, command.Type, command.Description, command.TransactionDate }, cancellationToken);
        return tx.Id;
    }
}
