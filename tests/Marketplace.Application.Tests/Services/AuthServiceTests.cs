using AutoMapper;
using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Common.Mappings;
using Marketplace.Application.DTOs.Auth;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.ValueObjects;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwt = new();
    private readonly Mock<IMessageBus> _bus = new();

    private readonly IMapper _mapper;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mapper = new MapperConfiguration(cfg =>
            cfg.AddProfile<MappingProfile>())
            .CreateMapper();

        _service = new AuthService(
            _userRepo.Object,
            _passwordHasher.Object,
            _jwt.Object,
            _mapper,
            _bus.Object,
            Mock.Of<ILogger<AuthService>>());
    }

    private void SetupTokens(User user)
    {
        _jwt
            .Setup(j => j.GenerateAccessToken(user))
            .Returns("access_token");

        _jwt
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");
    }

    [Fact]
    public async Task Register_NewUser_ShouldReturnAuthResponseAndPublishEvent()
    {
        _userRepo
            .Setup(r => r.GetByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasher
            .Setup(h => h.Hash("Pass123!"))
            .Returns("hashed");

        _userRepo
            .Setup(r => r.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _userRepo
            .Setup(r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _jwt
            .Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access_token");

        _jwt
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        var result = await _service.RegisterAsync(
            new RegisterRequest
            {
                Email = "TEST@Test.com",
                Password = "Pass123!",
                ConfirmPassword = "Pass123!",
                FirstName = "John",
                LastName = "Doe"
            });

        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.TokenType.Should().Be("Bearer");

        result.User.Email.Should().Be("test@test.com");
        result.User.FirstName.Should().Be("John");
        result.User.LastName.Should().Be("Doe");
        result.User.Role.Should().Be(UserRole.Customer);

        _bus.Verify(
            b => b.Publish(
                "user.registered",
                It.Is<string>(message =>
                    message.Contains("test@test.com"))),
            Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldThrowConflict()
    {
        var existingUser = new User(
            new Email("test@test.com"),
            "hash",
            "John",
            "Doe");

        _userRepo
            .Setup(r => r.GetByEmailAsync(
                "test@test.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(
                new RegisterRequest
                {
                    Email = "test@test.com",
                    Password = "Pass123!",
                    ConfirmPassword = "Pass123!",
                    FirstName = "A",
                    LastName = "B"
                }));
    }

    [Fact]
    public async Task Register_AdminRole_ShouldBecomeCustomer()
    {
        _userRepo
            .Setup(r => r.GetByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hash");

        _userRepo
            .Setup(r => r.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _userRepo
            .Setup(r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _jwt
            .Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access_token");

        _jwt
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        var result = await _service.RegisterAsync(
            new RegisterRequest
            {
                Email = "admin@example.com",
                Password = "Pass123!",
                ConfirmPassword = "Pass123!",
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Admin
            });

        result.User.Role.Should().Be(UserRole.Customer);
    }

    [Fact]
    public async Task Register_WithoutRole_ShouldUseCustomer()
    {
        _userRepo
            .Setup(r => r.GetByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hash");

        _userRepo
            .Setup(r => r.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _userRepo
            .Setup(r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _jwt
            .Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access_token");

        _jwt
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Test",
            LastName = "User",
            Role = null
        };

        var result = await _service.RegisterAsync(request);

        result.User.Role.Should().Be(UserRole.Customer);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldThrowUnauthorized()
    {
        _userRepo
            .Setup(r => r.GetByEmailAsync(
                "a@b.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(
                new LoginRequest("a@b.com", "bad")));
    }

    [Fact]
    public async Task Login_InvalidPassword_ShouldThrowUnauthorized()
    {
        var user = new User(
            new Email("test@test.com"),
            "correct_hash",
            "John",
            "Doe");

        _userRepo
            .Setup(r => r.GetByEmailAsync(
                user.Email.Value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify(
                "wrong_password",
                "correct_hash"))
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(
                new LoginRequest(
                    user.Email.Value,
                    "wrong_password")));
    }

    [Fact]
    public async Task Login_UnapprovedSeller_ShouldThrowUnauthorized()
    {
        var seller = new User(
            new Email("seller@example.com"),
            "hash",
            "Seller",
            "User",
            UserRole.Seller);

        _userRepo
            .Setup(r => r.GetByEmailAsync(
                seller.Email.Value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(seller);

        _passwordHasher
            .Setup(h => h.Verify(
                "Pass123!",
                "hash"))
            .Returns(true);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(
                new LoginRequest(
                    seller.Email.Value,
                    "Pass123!")));
    }

    [Fact]
    public async Task Login_Valid_ShouldReturnAuthResponseAndPublishEvent()
    {
        var user = new User(
            new Email("test@test.com"),
            "hash",
            "John",
            "Doe");

        SetupTokens(user);

        _userRepo
            .Setup(r => r.GetByEmailAsync(
                user.Email.Value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify(
                "Pass123!",
                "hash"))
            .Returns(true);

        var result = await _service.LoginAsync(
            new LoginRequest(
                user.Email.Value,
                "Pass123!"));

        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.TokenType.Should().Be("Bearer");
        result.User.Email.Should().Be("test@test.com");
        result.User.Role.Should().Be(UserRole.Customer);

        _bus.Verify(
            b => b.Publish(
                "user.loggedin",
                It.Is<string>(message =>
                    message.Contains(user.Id.ToString()))),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_NotFound_ShouldThrow()
    {
        _userRepo
            .Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetCurrentUserAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser()
    {
        var user = new User(
            new Email("test@test.com"),
            "hash",
            "John",
            "Doe",
            UserRole.Customer);

        _userRepo
            .Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.GetCurrentUserAsync(user.Id);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@test.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Role.Should().Be(UserRole.Customer);
    }
}
