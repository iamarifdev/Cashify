using Cashify.Api.Database;
using Cashify.Api.Entities;
using Cashify.Api.Infrastructure;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Auth.GoogleSignIn;

public class GoogleSignInHandler
{
    private readonly AppDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public GoogleSignInHandler(AppDbContext dbContext, JwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public async Task<GoogleSignInResponse> Handle(GoogleSignInRequest request, CancellationToken cancellationToken)
    {
        var clientIds = _configuration.GetSection("Google:ClientIds").Get<string[]>();
        var clientId = _configuration.GetValue<string>("Google:ClientId");
        var allowedAudiences = clientIds?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if ((allowedAudiences == null || allowedAudiences.Length == 0) && !string.IsNullOrWhiteSpace(clientId))
        {
            allowedAudiences = [clientId];
        }

        var payload = await GoogleJsonWebSignature.ValidateAsync(
            request.IdToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = allowedAudiences,
                HostedDomain = null,
                Clock = null
            });

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.GoogleUserId == payload.Subject, cancellationToken);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                GoogleUserId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name ?? payload.Email,
                PhotoUrl = payload.Picture,
                EmailVerified = payload.EmailVerified,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(user);
        }
        else
        {
            user.Name = payload.Name ?? user.Name;
            user.PhotoUrl = payload.Picture ?? user.PhotoUrl;
            user.EmailVerified = payload.EmailVerified;
            user.LastLoginAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var userInfo = new UserInfo(
            Id: user.Id.ToString(),
            Name: user.Name,
            Email: user.Email,
            PhotoUrl: user.PhotoUrl,
            EmailVerified: user.EmailVerified,
            HasCompletedOnboarding: user.HasCompletedOnboarding
        );

        var (token, expiresIn) = _jwtTokenService.CreateTokenWithExpiration(user);
        return new GoogleSignInResponse(userInfo, token, expiresIn);
    }
}
