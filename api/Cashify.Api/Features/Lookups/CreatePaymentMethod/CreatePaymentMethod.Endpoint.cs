using Cashify.Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Lookups.CreatePaymentMethod;

public class CreatePaymentMethodEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/lookups/payment-methods",
                [Authorize] async ([FromBody] CreatePaymentMethodCommand command,
                    IUserContext userContext,
                    CreatePaymentMethodHandler handler,
                    IValidator<CreatePaymentMethodCommand> validator,
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

                    return Results.Created($"/lookups/payment-methods/{id}", new { id, command.Name });
                })
            .WithTags("Lookups")
            .WithDocs("Create payment method", "Creates a payment method for the business.");
    }
}
