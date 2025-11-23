namespace Cashify.Api.Features.Reports.ExportTransactions;

public record ExportTransactionsQuery(Guid BusinessId, Guid CashbookId, string Format = "csv");
