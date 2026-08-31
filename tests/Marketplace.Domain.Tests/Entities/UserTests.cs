using FluentAssertions;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Events;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateRegisteredEvent()
    {
        var user = new User(new Email("user@example.com"), "hash", "John", "Doe");
        user.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UserRegisteredEvent>();
        user.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void Seller_ShouldStartUnapproved()
    {
        var user = new User(new Email("seller@example.com"), "hash", "John", "Doe", UserRole.Seller);
        user.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void ProfileAndStatus_ShouldChange()
    {
        var user = new User(new Email("user@example.com"), "hash", "John", "Doe");
        user.UpdateProfile("Jane", "Smith", "+49123456789");
        user.FullName.Should().Be("Jane Smith");
        user.Deactivate();
        user.IsActive.Should().BeFalse();
        user.Activate();
        user.IsActive.Should().BeTrue();
    }
}
