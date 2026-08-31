using FluentAssertions;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void ValidEmail_ShouldNormalizeToLowerCase()
    {
        var email = new Email("USER@Example.COM");
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void InvalidEmail_ShouldThrow()
    {
        Action act = () => new Email("invalid");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_ShouldBeByValue()
    {
        new Email("a@example.com").Should().Be(new Email("a@example.com"));
    }
}
