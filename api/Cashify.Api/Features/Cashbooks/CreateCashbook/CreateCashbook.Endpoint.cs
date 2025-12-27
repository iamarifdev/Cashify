using Cashify.Api.Infrastructure;
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
                    IUserContext userContext,
                    CreateCashbookHandler handler,
                    IValidator<CreateCashbookCommand> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(command, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var userId = userContext.GetUserId();
                    var cashbookId = await handler.Handle(id, userId, command, ct);
                    if (!cashbookId.HasValue)
                    {
                        return Results.Forbid();
                    }

                    return Results.Created($"/businesses/{id}/cashbooks/{cashbookId}", new { id = cashbookId, name = command.Name });
                })
            .WithTags("Cashbooks")
            .WithDocs("Create cashbook", "Creates a cashbook under a business and assigns the caller as owner.");
    }
}
