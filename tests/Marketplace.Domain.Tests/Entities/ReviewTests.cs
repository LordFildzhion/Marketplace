using FluentAssertions;
using Marketplace.Domain.Entities;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.Entities;

public class ReviewTests
{
    [Fact]
    public void Constructor_ShouldSetFields()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), new Rating(5), "Great");
        review.Rating.Value.Should().Be(5);
        review.Comment.Should().Be("Great");
    }

    [Fact]
    public void SetComment_TooShort_ShouldThrow()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), new Rating(5), "Great");
        Action act = () => review.SetComment("No");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Response_ShouldBeAddedAndRemoved()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), new Rating(5), "Great");
        var seller = Guid.NewGuid();
        review.AddResponse("Thanks", seller);
        review.SellerResponse.Should().Be("Thanks");
        review.RespondedBy.Should().Be(seller);
        review.RemoveResponse();
        review.SellerResponse.Should().BeNull();
    }

    [Fact]
    public void ImageUrls_ShouldRoundTrip()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), new Rating(5), "Great");
        review.SetImageUrls(new List<string> { "a.jpg", "b.jpg" });
        review.GetImageUrls().Should().Equal("a.jpg", "b.jpg");
    }
}
