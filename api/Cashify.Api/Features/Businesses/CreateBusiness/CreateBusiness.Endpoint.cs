using Cashify.Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Businesses.CreateBusiness;

public class CreateBusinessEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/businesses",
                [Authorize] async ([FromBody] CreateBusinessCommand command,
                    IUserContext userContext,
                    CreateBusinessHandler handler,
                    IValidator<CreateBusinessCommand> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(command, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var userId = userContext.GetUserId();
                    var businessId = await handler.Handle(command, userId, ct);
                    return Results.Created($"/businesses/{businessId}", new { id = businessId, name = command.Name });
                })
            .WithTags("Businesses")
            .WithDocs("Create business", "Creates a new business and assigns the caller as owner.");
    }
}
