using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Cashbooks.CreateCashbook;

public class CreateCashbookEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/businesses/{id:guid}/cashbooks",
                [Authorize] async (Guid id,
                    [FromBody] CreateCashbookCommand command,
                    HttpContext context,
                    CreateCashbookHandler handler,
                    IValidator<CreateCashbookCommand> validator,
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

                    var cashbookId = await handler.Handle(id, userId, command, ct);
                    if (!cashbookId.HasValue)
                    {
                        return Results.Forbid();
                    }

                    return Results.Created($"/businesses/{id}/cashbooks/{cashbookId}", new { id = cashbookId, name = command.Name });
                })
            .WithTags("Cashbooks");
    }
}
