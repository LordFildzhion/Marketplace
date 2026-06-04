
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Marketplace.Infrastructure.Auth;

public class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid UserId => Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    public string Email => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value!;
    public string Role => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value!;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
