using Cashify.Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Transactions.CreateTransaction;

public class CreateTransactionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/transactions",
                [Authorize] async (Guid businessId,
                    Guid cashbookId,
                    [FromBody] CreateTransactionCommand command,
                    IUserContext userContext,
                    CreateTransactionHandler handler,
                    IValidator<CreateTransactionCommand> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(command, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var userId = userContext.GetUserId();
                    var id = await handler.Handle(businessId, cashbookId, userId, command, ct);
                    if (!id.HasValue)
                    {
                        return Results.Forbid();
                    }

                    return Results.Created($"/businesses/{businessId}/cashbooks/{cashbookId}/transactions/{id}", new { id });
                })
            .WithTags("Transactions")
            .WithDocs("Create transaction", "Creates a transaction in the cashbook with optional inline lookup creation.");
    }
}
