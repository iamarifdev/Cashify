using Cashify.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Transactions.GetTransactionChanges;

public class GetTransactionChangesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/transactions/{transactionId:guid}/changes",
                [Authorize] async (Guid businessId, Guid cashbookId, Guid transactionId, IUserContext userContext, GetTransactionChangesHandler handler, CancellationToken ct) =>
                {
                    var userId = userContext.GetUserId();
                    var changes = await handler.Handle(businessId, cashbookId, transactionId, userId, ct);
                    if (!changes.Any())
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(changes);
                })
            .WithTags("Transactions")
            .WithDocs("Transaction change history", "Returns the recorded change history for a transaction.");
    }
}
