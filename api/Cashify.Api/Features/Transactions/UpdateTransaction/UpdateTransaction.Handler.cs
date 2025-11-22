using System.Text.Json;
using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Transactions.UpdateTransaction;

public class UpdateTransactionHandler
{
    private readonly AppDbContext _dbContext;

    public UpdateTransactionHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(Guid businessId, Guid cashbookId, Guid transactionId, Guid userId, UpdateTransactionCommand command, CancellationToken cancellationToken)
    {
        var tx = await _dbContext.Transactions.FirstOrDefaultAsync(
            x => x.Id == transactionId && x.BusinessId == businessId && x.CashbookId == cashbookId,
            cancellationToken);

        if (tx == null)
        {
            return false;
        }

        var changes = new Dictionary<string, object?>();

        void Track<T>(string field, T oldValue, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                changes[field] = new { old = oldValue, @new = newValue };
            }
        }

        Track("amount", tx.Amount, command.Amount);
        Track("type", tx.Type, command.Type);
        Track("categoryId", tx.CategoryId, command.CategoryId);
        Track("contactId", tx.ContactId, command.ContactId);
        Track("paymentMethodId", tx.PaymentMethodId, command.PaymentMethodId);
        Track("description", tx.Description, command.Description);
        Track("transactionDate", tx.TransactionDate, command.TransactionDate);

        tx.Amount = command.Amount;
        tx.Type = command.Type;
        tx.CategoryId = command.CategoryId;
        tx.ContactId = command.ContactId;
        tx.PaymentMethodId = command.PaymentMethodId;
        tx.Description = command.Description;
        tx.TransactionDate = command.TransactionDate;
        tx.UpdatedAt = DateTime.UtcNow;
        tx.UpdatedByUserId = userId;
        tx.Version += 1;

        if (changes.Any())
        {
            _dbContext.TransactionChanges.Add(new Entities.TransactionChange
            {
                Id = Guid.NewGuid(),
                TransactionId = tx.Id,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                ChangeType = "updated",
                ChangesJson = JsonSerializer.Serialize(changes)
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
