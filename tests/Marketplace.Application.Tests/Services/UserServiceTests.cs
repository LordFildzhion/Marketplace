using AutoMapper;
using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Common.Mappings;
using Marketplace.Application.DTOs.Users;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly IMapper _mapper;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _service = new UserService(_userRepo.Object, _hasher.Object, _mapper, Mock.Of<ILogger<UserService>>());
    }

    private static User User(UserRole role = UserRole.Customer)
        => new(new Email("user@example.com"), "hash", "John", "Doe", role);

    [Fact]
    public async Task GetById_NotFound_ShouldThrow()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetById_ShouldMapUser()
    {
        var user = User();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        var result = await _service.GetByIdAsync(user.Id);
        result.Email.Should().Be("user@example.com");
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task UpdateProfile_ShouldPersistChanges()
    {
        var user = User();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        var result = await _service.UpdateProfileAsync(user.Id, new UpdateProfileRequest { FirstName = "Jane", Phone = "+49123456789" });
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Doe");
        _userRepo.Verify(r => r.UpdateAsync(user, default), Times.Once);
    }

    [Fact]
    public async Task GetUsers_ShouldFilterByRoleAndSearch()
    {
        var seller = User(UserRole.Seller);
        var customer = new User(new Email("alice@example.com"), "hash", "Alice", "Smith", UserRole.Customer);
        _userRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new[] { seller, customer });
        var result = await _service.GetUsersAsync(new UserFilterRequest { Role = "Seller", Search = "user" });
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task ApproveSeller_NonSeller_ShouldThrow()
    {
        var user = User(UserRole.Customer);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        await Assert.ThrowsAsync<AppException>(() => _service.ApproveSellerAsync(user.Id));
    }

    [Fact]
    public async Task ApproveSeller_ShouldApproveAndSave()
    {
        var user = User(UserRole.Seller);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        await _service.ApproveSellerAsync(user.Id);
        user.IsApproved.Should().BeTrue();
        _userRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DisapproveSeller_ShouldDisapproveAndSave()
    {
        var user = User(UserRole.Seller);
        user.Approve();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        await _service.DisapproveSellerAsync(user.Id);
        user.IsApproved.Should().BeFalse();
        _userRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldHashAndPersistNewPassword()
    {
        var user = User();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("new-hash");
        var password = await _service.ResetUserPasswordAsync(user.Id);
        password.Should().HaveLength(12);
        _hasher.Verify(h => h.Hash(It.Is<string>(s => s.Length == 12)), Times.Once);
        user.PasswordHash.Should().Be("new-hash");
    }
}
