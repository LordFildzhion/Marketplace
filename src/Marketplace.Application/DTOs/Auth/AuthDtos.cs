namespace Marketplace.Application.DTOs.Auth;

using Marketplace.Domain.Enums;

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public UserRole? Role { get; init; }
}

public record LoginRequest(string Email, string Password, bool RememberMe = false);
public record TokenRefreshRequest(string AccessToken, string RefreshToken);

public class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public required UserBriefDto User { get; init; }
}
public class UserBriefDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public UserRole Role { get; init; }
}
