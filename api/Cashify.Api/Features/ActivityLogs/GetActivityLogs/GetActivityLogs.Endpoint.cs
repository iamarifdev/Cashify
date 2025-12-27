using Cashify.Api.Features;
using Cashify.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.ActivityLogs.GetActivityLogs;

public class GetActivityLogsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/activity",
                [Authorize] async ([AsParameters] GetActivityLogsQuery query, IUserContext userContext, GetActivityLogsHandler handler, CancellationToken ct) =>
                {
                    var userId = userContext.GetUserId();
                    var logs = await handler.Handle(query, userId, ct);
                    if (logs is null)
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(logs);
                })
            .WithTags("Activity")
            .WithDocs("Activity logs", "Returns recent activity entries for the business, optionally filtered by cashbook.");
    }
}
