using FluentAssertions;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.ValueObjects;

public class DimensionsTests
{
    [Fact]
    public void ValidDimensions_ShouldCalculateVolume()
    {
        var d = new Dimensions(2, 3, 4, 1);
        d.Volume.Should().Be(24);
    }

    [Fact] public void NegativeLength_ShouldThrow() => ((Action)(() => new Dimensions(-1, 2, 3))).Should().Throw<ArgumentException>();
    [Fact] public void ZeroWeight_ShouldThrow() => ((Action)(() => new Dimensions(1, 2, 3, 0))).Should().Throw<ArgumentException>();
}
