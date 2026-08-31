using FluentAssertions;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.ValueObjects;

public class RatingTests
{
    [Fact] public void OneToFive_ShouldBeValid() => new Rating(5).Value.Should().Be(5);
    [Fact] public void Zero_ShouldThrow() => ((Action)(() => new Rating(0))).Should().Throw<ArgumentException>();
    [Fact] public void Six_ShouldThrow() => ((Action)(() => new Rating(6))).Should().Throw<ArgumentException>();
    [Fact] public void Comparison_ShouldWork() => (new Rating(5) > new Rating(3)).Should().BeTrue();
}
