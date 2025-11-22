using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Cashify.Api.Features.Businesses.CreateBusiness;

public class CreateBusinessEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/businesses",
                [Authorize] async ([FromBody] CreateBusinessCommand command,
                    HttpContext context,
                    CreateBusinessHandler handler,
                    IValidator<CreateBusinessCommand> validator,
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

                    var businessId = await handler.Handle(command, userId, ct);
                    return Results.Created($"/businesses/{businessId}", new { id = businessId, name = command.Name });
                })
            .WithTags("Businesses");
    }
}
