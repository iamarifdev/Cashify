using System.IdentityModel.Tokens.Jwt;
using Cashify.Api.Features;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Businesses.AddMember;

public class AddMemberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/businesses/{id:guid}/members",
                [Authorize] async (Guid id,
                    [FromBody] AddMemberCommand command,
                    HttpContext context,
                    AddMemberHandler handler,
                    IValidator<AddMemberCommand> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(command, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var actingUserId))
                    {
                        return Results.Unauthorized();
                    }

                    var ok = await handler.Handle(id, actingUserId, command, ct);
                    return ok ? Results.NoContent() : Results.Forbid();
                })
            .WithTags("Businesses")
            .WithDocs("Add business member", "Adds a member to a business; only owners may add members.");
    }
}
