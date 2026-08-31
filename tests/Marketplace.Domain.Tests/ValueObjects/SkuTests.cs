using FluentAssertions;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.ValueObjects;

public class SkuTests
{
    [Fact] public void ValidSku_ShouldKeepValue() => new Sku("ABC-123").Value.Should().Be("ABC-123");
    [Fact] public void Lowercase_ShouldThrow() => ((Action)(() => new Sku("abc"))).Should().Throw<ArgumentException>();
    [Fact] public void TooShort_ShouldThrow() => ((Action)(() => new Sku("AB"))).Should().Throw<ArgumentException>();
    [Fact] public void InvalidCharacters_ShouldThrow() => ((Action)(() => new Sku("ABC_123"))).Should().Throw<ArgumentException>();
}
