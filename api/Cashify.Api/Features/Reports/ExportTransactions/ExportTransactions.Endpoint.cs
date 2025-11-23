using System.IdentityModel.Tokens.Jwt;
using Cashify.Api.Features;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Reports.ExportTransactions;

public class ExportTransactionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/reports/export",
                [Authorize] async ([AsParameters] ExportTransactionsQuery query, HttpContext context, ExportTransactionsHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

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
