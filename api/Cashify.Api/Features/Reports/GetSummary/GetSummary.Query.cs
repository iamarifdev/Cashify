namespace Cashify.Api.Features.Reports.GetSummary;

public record GetSummaryQuery(
    Guid BusinessId,
    Guid? CashbookId,
    string Range = "monthly",
    DateTime? StartDate = null,
    DateTime? EndDate = null);
