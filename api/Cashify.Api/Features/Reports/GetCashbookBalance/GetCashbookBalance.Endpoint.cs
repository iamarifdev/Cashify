using Cashify.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Reports.GetCashbookBalance;

public class GetCashbookBalanceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/reports/balance",
                [Authorize] async (Guid businessId, Guid cashbookId, IUserContext userContext, GetCashbookBalanceHandler handler, CancellationToken ct) =>
                {
                    var userId = userContext.GetUserId();
                    var balance = await handler.Handle(businessId, cashbookId, userId, ct);
                    if (balance is null)
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(new { balance });
                })
            .WithTags("Reports")
            .WithDocs("Cashbook balance", "Returns income - expense + transfer for the specified cashbook.");
    }
}
