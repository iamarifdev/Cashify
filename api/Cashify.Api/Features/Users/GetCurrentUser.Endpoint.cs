using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Users;

public class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me", [Authorize] (HttpContext context) =>
            {
                var claims = context.User;
                var userId = claims.FindFirst("sub")?.Value ?? claims.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                var email = claims.FindFirst("email")?.Value;
                var name = claims.FindFirst("name")?.Value;
                var picture = claims.FindFirst("picture")?.Value;

                return Results.Ok(new { userId, email, name, picture });
            })
            .WithTags("Users");
    }
}
