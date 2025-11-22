using FluentValidation;

namespace Cashify.Api.Features.Auth.GoogleSignIn;

public class GoogleSignInValidator : AbstractValidator<GoogleSignInRequest>
{
    public GoogleSignInValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}

