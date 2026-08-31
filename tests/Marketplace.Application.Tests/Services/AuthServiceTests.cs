using AutoMapper;
using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Common.Mappings;
using Marketplace.Application.DTOs.Auth;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
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
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _service = new AuthService(_userRepo.Object, _passwordHasher.Object, _jwt.Object, _mapper, _bus.Object, Mock.Of<ILogger<AuthService>>());
    }

    private void SetupTokens(User user)
    {
        _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("access_token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");
    }

    [Fact]
    public async Task Register_NewUser_ShouldReturnAuthResponseAndPublishEvent()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash("Pass123!")) .Returns("hashed");
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default)).ReturnsAsync((User u, CancellationToken _) => u);
        _userRepo.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");

        var result = await _service.RegisterAsync(new RegisterRequest
        {
            Email = "TEST@Test.com", Password = "Pass123!", ConfirmPassword = "Pass123!",
            FirstName = "John", LastName = "Doe"
        });

        result.AccessToken.Should().Be("access_token");
        result.User.Email.Should().Be("test@test.com");
        _bus.Verify(b => b.Publish("user.registered", It.Is<string>(s => s.Contains("test@test.com"))), Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldThrowConflict()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com", default)).ReturnsAsync(
            new User(new Email("test@test.com"), "hash", "John", "Doe"));
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(new RegisterRequest { Email = "test@test.com", Password = "Pass123!", ConfirmPassword = "Pass123!", FirstName = "A", LastName = "B" }));
    }

    [Fact]
    public async Task Register_AdminRole_ShouldBecomeCustomer()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash");
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default)).ReturnsAsync((User u, CancellationToken _) => u);
        _userRepo.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("a");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("r");
        var result = await _service.RegisterAsync(new RegisterRequest { Email = "a@b.com", Password = "Pass123!", ConfirmPassword = "Pass123!", FirstName = "A", LastName = "B", Role = "Admin" });
        result.User.Role.Should().Be(nameof(UserRole.Customer));
    }

    [Fact]
    public async Task Register_InvalidRole_ShouldThrowValidation()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<Marketplace.Application.Common.Exceptions.ValidationException>(() =>
            _service.RegisterAsync(new RegisterRequest { Email = "a@b.com", Password = "Pass123!", ConfirmPassword = "Pass123!", FirstName = "A", LastName = "B", Role = "Manager" }));
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldThrowUnauthorized()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("a@b.com", default)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(new LoginRequest("a@b.com", "bad")));
    }

    [Fact]
    public async Task Login_UnapprovedSeller_ShouldThrowUnauthorized()
    {
        var seller = new User(new Email("seller@example.com"), "hash", "S", "Seller", UserRole.Seller);
        _userRepo.Setup(r => r.GetByEmailAsync(seller.Email.Value, default)).ReturnsAsync(seller);
        _passwordHasher.Setup(h => h.Verify("Pass123!", "hash")).Returns(true);
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(new LoginRequest(seller.Email.Value, "Pass123!")));
    }

    [Fact]
    public async Task Login_Valid_ShouldPublishEvent()
    {
        var user = new User(new Email("test@test.com"), "hash", "John", "Doe");
        SetupTokens(user);
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email.Value, default)).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("Pass123!", "hash")).Returns(true);

        var result = await _service.LoginAsync(new LoginRequest(user.Email.Value, "Pass123!"));
        result.AccessToken.Should().Be("access_token");
        _bus.Verify(b => b.Publish("user.loggedin", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_NotFound_ShouldThrow()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCurrentUserAsync(Guid.NewGuid()));
    }
}
