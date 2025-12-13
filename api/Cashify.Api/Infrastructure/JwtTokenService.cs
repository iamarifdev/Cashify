using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cashify.Api.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Cashify.Api.Infrastructure;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string token, long expiresIn) CreateTokenWithExpiration(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? "cashify-api";
        var audience = jwtSection["Audience"] ?? "cashify-app";
        var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is missing");
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new("picture", user.PhotoUrl ?? string.Empty)
        };

        var expirationTime = DateTime.UtcNow.AddHours(1);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expirationTime,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresIn = (long)(expirationTime - DateTime.UtcNow).TotalSeconds;

        return (tokenString, expiresIn);
    }
}
