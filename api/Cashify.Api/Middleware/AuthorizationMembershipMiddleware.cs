using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Cashify.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Middleware;

/// <summary>
/// Enriches the user principal with membership claims for authorization policies.
/// </summary>
public class AuthorizationMembershipMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMembershipMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, AppDbContext dbContext)
    {
        var userIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (Guid.TryParse(userIdValue, out var userId))
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            Guid? businessId = null;
            Guid? cashbookId = null;

            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (segments[i].Equals("businesses", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(segments[i + 1], out var bId))
                {
                    businessId = bId;
                }

                if (segments[i].Equals("cashbooks", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(segments[i + 1], out var cId))
                {
                    cashbookId = cId;
                }
            }

            if (businessId.HasValue)
            {
                var isBusinessMember = await dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == businessId && x.UserId == userId);
                if (isBusinessMember && !context.User.HasClaim(c => c.Type == "biz_member"))
                {
                    var identity = context.User.Identity as ClaimsIdentity;
                    identity?.AddClaim(new Claim("biz_member", "true"));
                }
            }

            if (cashbookId.HasValue)
            {
                var isCashbookMember = await dbContext.CashbookMembers.AnyAsync(x => x.CashbookId == cashbookId && x.UserId == userId);
                if (isCashbookMember && !context.User.HasClaim(c => c.Type == "cb_member"))
                {
                    var identity = context.User.Identity as ClaimsIdentity;
                    identity?.AddClaim(new Claim("cb_member", "true"));
                }
            }
        }

        await _next(context);
    }
}
