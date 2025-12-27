using Cashify.Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Auth.CompleteOnboarding;

public class CompleteOnboardingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/onboarding",
                [Authorize] async ([FromBody] CompleteOnboardingRequest request,
                    IUserContext userContext,
                    CompleteOnboardingHandler handler,
                    IValidator<CompleteOnboardingRequest> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(request, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var userId = userContext.GetUserId();
                    var result = await handler.Handle(userId, request, ct);
                    return Results.Ok(result);
                })
            .WithTags("Auth")
            .WithDocs("Complete onboarding", "Updates the user's profile and creates a default business if missing.");
    }
}
