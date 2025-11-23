using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Auth.GoogleSignIn;

public class GoogleSignInEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/google",
                async ([FromBody] GoogleSignInRequest request,
                    GoogleSignInHandler handler,
                    IValidator<GoogleSignInRequest> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(request, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var response = await handler.Handle(request, ct);
                    return Results.Ok(response);
                })
            .AllowAnonymous()
            .WithTags("Auth")
            .WithDocs("Google sign-in", "Validates a Google ID token, upserts the user, and issues a JWT for subsequent requests.");
    }
}
