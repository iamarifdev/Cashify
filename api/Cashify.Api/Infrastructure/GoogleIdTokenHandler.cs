using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Cashify.Api.Database;
using Cashify.Api.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cashify.Api.Infrastructure;

/// <summary>
/// Authenticates incoming Google ID tokens by validating issuer, audience, expiry, and signature,
/// then maps the Google subject to an app user (creating one if missing) and re-emits claims with
/// the app user ID as <c>sub</c>.
/// </summary>
public class GoogleIdTokenHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly GoogleJsonWebSignature.ValidationSettings _validationSettings;
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    [Obsolete("Obsolete")]
    public GoogleIdTokenHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        GoogleJsonWebSignature.ValidationSettings validationSettings,
        AppDbContext db,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        _validationSettings = validationSettings;
        _db = db;
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }

        try
        {
            var configuredAudiences = (_configuration.GetSection("Google:ClientIds").Get<string[]>() ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
            if (configuredAudiences.Length > 0)
            {
                _validationSettings.Audience = configuredAudiences;
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, _validationSettings);

            var googleSub = payload.Subject;
            if (string.IsNullOrWhiteSpace(googleSub))
            {
                return AuthenticateResult.Fail("Missing Google subject");
            }

            var cancellation = Context.RequestAborted;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleUserId == googleSub, cancellation);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    GoogleUserId = googleSub,
                    Email = payload.Email,
                    Name = payload.Name ?? payload.Email ?? "New User",
                    PhotoUrl = payload.Picture,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                _db.Users.Add(user);
            }
            else
            {
                user.Name = payload.Name ?? user.Name;
                user.PhotoUrl = payload.Picture ?? user.PhotoUrl;
                user.LastLoginAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(cancellation);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            }

            if (!string.IsNullOrWhiteSpace(user.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Name));
            }

            if (!string.IsNullOrWhiteSpace(user.PhotoUrl))
            {
                claims.Add(new Claim("picture", user.PhotoUrl));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (InvalidJwtException ex)
        {
            return AuthenticateResult.Fail($"Google token invalid: {ex.Message}");
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail($"Google token processing failed: {ex.Message}");
        }
    }
}
