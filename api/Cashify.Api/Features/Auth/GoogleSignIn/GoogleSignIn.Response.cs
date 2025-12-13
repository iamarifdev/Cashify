namespace Cashify.Api.Features.Auth.GoogleSignIn;

public record GoogleSignInResponse(
    UserInfo User,
    string Token,
    long ExpiresIn);

public record UserInfo(
    string Id,
    string Name,
    string Email,
    string? PhotoUrl,
    bool EmailVerified,
    bool HasCompletedOnboarding);
