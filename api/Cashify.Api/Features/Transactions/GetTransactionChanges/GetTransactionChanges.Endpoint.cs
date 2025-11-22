using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Transactions.GetTransactionChanges;

public class GetTransactionChangesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/transactions/{transactionId:guid}/changes",
                [Authorize] async (Guid businessId, Guid cashbookId, Guid transactionId, HttpContext context, GetTransactionChangesHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var changes = await handler.Handle(businessId, cashbookId, transactionId, userId, ct);
                    if (!changes.Any())
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(changes);
                })
            .WithTags("Transactions");
    }
}
