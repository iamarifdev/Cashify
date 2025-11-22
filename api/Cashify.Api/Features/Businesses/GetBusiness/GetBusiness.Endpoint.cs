using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Businesses.GetBusiness;

public class GetBusinessEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{id:guid}",
                [Authorize] async (Guid id, HttpContext context, GetBusinessHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var business = await handler.Handle(id, userId, ct);
                    return business is null ? Results.NotFound() : Results.Ok(business);
                })
            .WithTags("Businesses");
    }
}
