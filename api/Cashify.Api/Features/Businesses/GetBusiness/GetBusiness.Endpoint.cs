using Cashify.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Businesses.GetBusiness;

public class GetBusinessEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{id:guid}",
                [Authorize] async (Guid id, IUserContext userContext, GetBusinessHandler handler, CancellationToken ct) =>
                {
                    var userId = userContext.GetUserId();
                    var business = await handler.Handle(id, userId, ct);
                    return business is null ? Results.NotFound() : Results.Ok(business);
                })
            .WithTags("Businesses")
            .WithDocs("Get business", "Gets a single business by ID for members.");
    }
}
