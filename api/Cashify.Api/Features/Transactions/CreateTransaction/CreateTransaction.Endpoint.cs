using System.IdentityModel.Tokens.Jwt;
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
                    HttpContext context,
                    CreateTransactionHandler handler,
                    IValidator<CreateTransactionCommand> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(command, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var id = await handler.Handle(businessId, cashbookId, userId, command, ct);
                    if (!id.HasValue)
                    {
                        return Results.Forbid();
                    }

                    return Results.Created($"/businesses/{businessId}/cashbooks/{cashbookId}/transactions/{id}", new { id });
                })
            .WithTags("Transactions");
    }
}
