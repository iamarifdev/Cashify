using System.Security.Claims;

namespace Cashify.Api.Infrastructure;

public interface IUserContext
{
    Guid GetUserId();
}

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var userIdValue = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID in token");
        
        return userId;
    }
}
