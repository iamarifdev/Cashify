using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Cashbooks.GetCashbooks;

public class GetCashbooksEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{id:guid}/cashbooks",
                [Authorize] async (Guid id, HttpContext context, GetCashbooksHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var result = await handler.Handle(id, userId, ct);
                    if (!result.IsMember)
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(result.Cashbooks);
                })
            .WithTags("Cashbooks")
            .WithDocs("List cashbooks", "Lists cashbooks for a business when the caller is a member.");
    }
}
