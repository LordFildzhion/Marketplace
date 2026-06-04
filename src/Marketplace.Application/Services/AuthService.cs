using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.DTOs.Auth;
using Marketplace.Application.DTOs.Users;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = new Email(request.Email);
        if (await _userRepository.GetByEmailAsync(request.Email, ct) != null)
            throw new ConflictException("User", "email", request.Email);

        var passwordHash = _passwordHasher.Hash(request.Password);
        var role = DetermineRole(request.Role);
        if (role == UserRole.Admin)
            role = UserRole.Customer;

        var user = new User(email, passwordHash, request.FirstName, request.LastName, role);
        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {Email} registered with role {Role}", request.Email, user.Role);
        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (user.Role == UserRole.Seller && !user.IsApproved)
            throw new UnauthorizedException("Your seller account is not yet approved by administrator.");

        _logger.LogInformation("User {Email} logged in", request.Email);
        return CreateAuthResponse(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("User", userId);
        return _mapper.Map<UserDto>(user);
    }

    private UserRole DetermineRole(string? roleString)
    {
        if (string.IsNullOrWhiteSpace(roleString))
            return UserRole.Customer;
        if (Enum.TryParse<UserRole>(roleString, true, out var parsed))
            return parsed;
        else
            throw new ValidationException("role", $"Invalid role: {roleString}. Allowed: Customer, Seller.");
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        return new AuthResponse
        {
            AccessToken = _jwtTokenGenerator.GenerateAccessToken(user),
            RefreshToken = _jwtTokenGenerator.GenerateRefreshToken(),
            TokenType = "Bearer",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            User = new UserBriefDto
            {
                Id = user.Id,
                Email = user.Email.Value,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString()
            }
        };
    }
}
