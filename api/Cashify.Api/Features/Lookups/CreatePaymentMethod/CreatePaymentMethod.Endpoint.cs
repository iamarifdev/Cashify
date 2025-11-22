using System.IdentityModel.Tokens.Jwt;
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
                    HttpContext context,
                    CreatePaymentMethodHandler handler,
                    IValidator<CreatePaymentMethodCommand> validator,
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

                    var id = await handler.Handle(command, userId, ct);
                    if (!id.HasValue)
                    {
                        return Results.Forbid();
                    }

                    return Results.Created($"/lookups/payment-methods/{id}", new { id, command.Name });
                })
            .WithTags("Lookups");
    }
}
