using Cashify.Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Lookups.CreateContact;

public class CreateContactEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/lookups/contacts",
                [Authorize] async ([FromBody] CreateContactCommand command,
                    IUserContext userContext,
                    CreateContactHandler handler,
                    IValidator<CreateContactCommand> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(command, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var userId = userContext.GetUserId();
                    var id = await handler.Handle(command, userId, ct);
                    if (!id.HasValue)
                    {
                        return Results.Forbid();
                    }

                    return Results.Created($"/lookups/contacts/{id}", new { id, command.Name, command.Type });
                })
            .WithTags("Lookups")
            .WithDocs("Create contact", "Creates a contact for the business (customer/supplier) with optional phone/email.");
    }
}
