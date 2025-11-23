using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Lookups.CreateCategory;

public class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/lookups/categories",
                [Authorize] async ([FromBody] CreateCategoryCommand command,
                    HttpContext context,
                    CreateCategoryHandler handler,
                    IValidator<CreateCategoryCommand> validator,
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

                    return Results.Created($"/lookups/categories/{id}", new { id, command.Name, command.Type });
                })
            .WithTags("Lookups")
            .WithDocs("Create category", "Creates a category for the business; type should align to income/expense/transfer.");
    }
}
