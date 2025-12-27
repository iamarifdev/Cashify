using Cashify.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Cashbooks.GetCashbooks;

public class GetCashbooksEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{id:guid}/cashbooks",
                [Authorize] async (Guid id, IUserContext userContext, GetCashbooksHandler handler, CancellationToken ct) =>
                {
                    var userId = userContext.GetUserId();
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
