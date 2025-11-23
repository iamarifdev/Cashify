using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Lookups.GetAllLookups;

public class GetAllLookupsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/lookups/all",
                [Authorize] async ([AsParameters] GetAllLookupsQuery query, HttpContext context, GetAllLookupsHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var result = await handler.Handle(query.BusinessId, userId, ct);
                    if (result is Array && ((Array)result).Length == 0)
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(result);
                })
            .WithTags("Lookups")
            .WithDocs("Get lookups", "Returns categories, contacts, and payment methods for the business (cached client-side).");
    }
}
