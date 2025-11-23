using System.IdentityModel.Tokens.Jwt;
using Cashify.Api.Features;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.ActivityLogs.GetActivityLogs;

public class GetActivityLogsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/activity",
                [Authorize] async ([AsParameters] GetActivityLogsQuery query, HttpContext context, GetActivityLogsHandler handler, CancellationToken ct) =>
                {
                    var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Unauthorized();
                    }

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
