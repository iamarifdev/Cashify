namespace Cashify.Api.Features.Auth.CompleteOnboarding;

public class CompleteOnboardingRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
}
