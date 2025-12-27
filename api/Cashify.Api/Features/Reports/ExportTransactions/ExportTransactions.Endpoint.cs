using Cashify.Api.Features;
using Cashify.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Reports.ExportTransactions;

public class ExportTransactionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/reports/export",
                [Authorize] async ([AsParameters] ExportTransactionsQuery query, IUserContext userContext, ExportTransactionsHandler handler, CancellationToken ct) =>
                {
                    var userId = userContext.GetUserId();
                    var result = await handler.Handle(query, userId, ct);
                    if (result is null)
                    {
                        return Results.Forbid();
                    }

                    var (contentType, data) = result.Value;
                    return Results.File(data, contentType, fileDownloadName: $"transactions.{query.Format}");
                })
            .WithTags("Reports")
            .WithDocs("Export transactions", "Exports transactions for the cashbook. Supports CSV; placeholder for Excel.");
    }
}
