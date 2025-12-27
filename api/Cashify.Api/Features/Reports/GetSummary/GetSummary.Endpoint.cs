using Cashify.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Cashify.Api.Features.Reports.GetSummary;

public class GetSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses/{businessId:guid}/reports/summary",
                [Authorize] async ([AsParameters] GetSummaryQuery query, IUserContext userContext, GetSummaryHandler handler, FluentValidation.IValidator<GetSummaryQuery> validator, CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(query, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var userId = userContext.GetUserId();
                    var result = await handler.Handle(query, userId, ct);
                    if (result is null)
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(result);
                })
            .WithTags("Reports")
            .WithDocs("Summary", "Returns income/expense/transfer totals for the business or a specific cashbook using UTC date filters.");
    }
}
