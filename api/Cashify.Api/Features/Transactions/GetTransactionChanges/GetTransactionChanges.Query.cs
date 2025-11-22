namespace Cashify.Api.Features.Transactions.GetTransactionChanges;

public record GetTransactionChangesQuery(Guid BusinessId, Guid CashbookId, Guid TransactionId);

