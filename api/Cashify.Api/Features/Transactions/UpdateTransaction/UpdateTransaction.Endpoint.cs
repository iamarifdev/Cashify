using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Transactions.UpdateTransaction;

public class UpdateTransactionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/transactions/{transactionId:guid}",
                [Authorize] async (Guid businessId,
                    Guid cashbookId,
                    Guid transactionId,
                    [FromBody] UpdateTransactionCommand command,
                    HttpContext context,
                    UpdateTransactionHandler handler,
                    IValidator<UpdateTransactionCommand> validator,
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

                    var ok = await handler.Handle(businessId, cashbookId, transactionId, userId, command, ct);
                    return ok ? Results.NoContent() : Results.NotFound();
                })
            .WithTags("Transactions");
    }
}
