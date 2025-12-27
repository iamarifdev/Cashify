using Cashify.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Businesses.GetBusinesses;

public class GetBusinessesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses",
                [Authorize] async (IUserContext userContext, GetBusinessesHandler handler, CancellationToken ct) =>
                {
                    var userId = userContext.GetUserId();
                    var businesses = await handler.Handle(userId, ct);
                    return Results.Ok(businesses);
                })
            .WithTags("Businesses")
            .WithDocs("List my businesses", "Returns businesses where the caller is a member, including their role.");
    }
}
