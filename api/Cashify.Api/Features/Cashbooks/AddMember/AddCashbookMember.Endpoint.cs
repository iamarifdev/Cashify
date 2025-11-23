using System.IdentityModel.Tokens.Jwt;
using Cashify.Api.Features;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Cashbooks.AddMember;

public class AddCashbookMemberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/members",
                [Authorize] async (Guid businessId,
                    Guid cashbookId,
                    [FromBody] AddCashbookMemberCommand command,
                    HttpContext context,
                    AddCashbookMemberHandler handler,
                    IValidator<AddCashbookMemberCommand> validator,
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

                    var ok = await handler.Handle(businessId, cashbookId, actingUserId, command, ct);
                    return ok ? Results.NoContent() : Results.Forbid();
                })
            .WithTags("Cashbooks")
            .WithDocs("Add cashbook member", "Adds a member to a cashbook; requires owner role on the business or cashbook.");
    }
}
