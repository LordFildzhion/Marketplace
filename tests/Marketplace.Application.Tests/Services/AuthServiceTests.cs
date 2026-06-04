using AutoMapper;
using FluentAssertions;
using Moq;
using Marketplace.Application.DTOs.Auth;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock = new();
    private readonly IMapper _mapper;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Marketplace.Application.Common.Mappings.MappingProfile>());
        _mapper = config.CreateMapper();
        _authService = new AuthService(
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _jwtGeneratorMock.Object,
            _mapper,
            Mock.Of<ILogger<AuthService>>());
    }

    [Fact]
    public async Task Register_NewUser_ShouldReturnAuthResponse()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _jwtGeneratorMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
        _jwtGeneratorMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");

        var request = new RegisterRequest { Email = "test@test.com", Password = "Pass123!", ConfirmPassword = "Pass123!", FirstName = "John", LastName = "Doe" };

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.User.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnAuthResponse()
    {
        var user = new User(new Email("test@test.com"), "hashed", "John", "Doe");
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com", default)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("Pass123!", "hashed")).Returns(true);
        _jwtGeneratorMock.Setup(j => j.GenerateAccessToken(user)).Returns("access_token");
        _jwtGeneratorMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");

        var result = await _authService.LoginAsync(new LoginRequest("test@test.com", "Pass123!"));

        result.AccessToken.Should().Be("access_token");
    }
}
