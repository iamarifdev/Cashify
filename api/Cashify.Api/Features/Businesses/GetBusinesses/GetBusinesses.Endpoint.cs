using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Businesses.GetBusinesses;

public class GetBusinessesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses",
                [Authorize] async (HttpContext context, GetBusinessesHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var businesses = await handler.Handle(userId, ct);
                    return Results.Ok(businesses);
                })
            .WithTags("Businesses");
    }
}
