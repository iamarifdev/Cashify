using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Transactions.GetTransactions;

public class GetTransactionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/transactions",
                [Authorize] async (Guid businessId, Guid cashbookId, HttpContext context, GetTransactionsHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var items = await handler.Handle(businessId, cashbookId, userId, ct);
                    if (!items.Any())
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(items);
                })
            .WithTags("Transactions")
            .WithDocs("List transactions", "Lists transactions for the specified cashbook, newest first.");
    }
}
