namespace Cashify.Api.Features.Transactions.GetTransactions;

public record GetTransactionsQuery(Guid BusinessId, Guid CashbookId);

