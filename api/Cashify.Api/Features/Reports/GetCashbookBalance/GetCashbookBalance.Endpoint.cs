using System.IdentityModel.Tokens.Jwt;
using Cashify.Api.Features;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Reports.GetCashbookBalance;

public class GetCashbookBalanceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/reports/balance",
                [Authorize] async (Guid businessId, Guid cashbookId, HttpContext context, GetCashbookBalanceHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

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
