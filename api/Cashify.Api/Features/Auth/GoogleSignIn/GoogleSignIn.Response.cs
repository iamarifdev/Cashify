namespace Cashify.Api.Features.Auth.GoogleSignIn;

public record GoogleSignInResponse(
    string Token,
    Guid UserId,
    string Email,
    string Name,
    string? PhotoUrl);

