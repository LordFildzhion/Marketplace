using FluentAssertions;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var address = new Address("USA", "New York", "Broadway", "123", "10001");
        address.Country.Should().Be("USA");
        address.City.Should().Be("New York");
    }

    [Fact]
    public void Constructor_EmptyCountry_ShouldThrow()
    {
        Action act = () => new Address("", "City", "Street", "1", "12345");
        act.Should().Throw<ArgumentException>();
    }
}
