using AutoMapper;
using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Common.Mappings;
using Marketplace.Application.DTOs.Reviews;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _reviewRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IFileStorageService> _storage = new();
    private readonly IMapper _mapper;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _service = new ReviewService(_reviewRepo.Object, _productRepo.Object, _storage.Object, _mapper, Mock.Of<ILogger<ReviewService>>());
    }

    private static Product Product() => new(new Sku("SKU"), "Phone", "Desc", new Money(100, "USD"), 10, Guid.NewGuid(), Guid.NewGuid());
    private static Review Review(Guid productId, Guid userId) => new(productId, userId, new Rating(5), "Great product");

    [Fact]
    public async Task CreateReview_ProductMissing_ShouldThrow()
    {
        var productId = Guid.NewGuid();
        _productRepo.Setup(r => r.GetByIdAsync(productId, default)).ReturnsAsync((Product?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateReviewAsync(productId, Guid.NewGuid(), 5, "Great", null));
    }

    [Fact]
    public async Task CreateReview_Valid_ShouldAddAndSave()
    {
        var product = Product();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _reviewRepo.Setup(r => r.AddAsync(It.IsAny<Review>(), default)).ReturnsAsync((Review r, CancellationToken _) => r);
        var result = await _service.CreateReviewAsync(product.Id, Guid.NewGuid(), 4, "Good", null);
        result.Rating.Should().Be(4);
        result.Comment.Should().Be("Good");
        _reviewRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetProductReviews_ShouldRequestFirst100()
    {
        var productId = Guid.NewGuid();
        _reviewRepo.Setup(r => r.GetProductReviewsAsync(productId, 1, 100, default))
            .ReturnsAsync((Array.Empty<Review>(), 0));
        (await _service.GetProductReviewsAsync(productId)).Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateReview_OtherUser_ShouldThrowForbidden()
    {
        var review = Review(Guid.NewGuid(), Guid.NewGuid());
        _reviewRepo.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.UpdateReviewAsync(review.Id, Guid.NewGuid(), new UpdateReviewRequest { Rating = 4 }));
    }

    [Fact]
    public async Task UpdateReview_Owner_ShouldUpdateAndSave()
    {
        var review = Review(Guid.NewGuid(), Guid.NewGuid());
        _reviewRepo.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
        var result = await _service.UpdateReviewAsync(review.Id, review.UserId, new UpdateReviewRequest { Rating = 3, Comment = "Okay" });
        result.Rating.Should().Be(3);
        result.Comment.Should().Be("Okay");
        _reviewRepo.Verify(r => r.UpdateAsync(review, default), Times.Once);
    }

    [Fact]
    public async Task DeleteReview_Admin_ShouldDelete()
    {
        var review = Review(Guid.NewGuid(), Guid.NewGuid());
        _reviewRepo.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
        await _service.DeleteReviewAsync(review.Id, Guid.NewGuid(), true);
        _reviewRepo.Verify(r => r.DeleteAsync(review, default), Times.Once);
        _reviewRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RespondToReview_ShouldStoreResponse()
    {
        var review = Review(Guid.NewGuid(), Guid.NewGuid());
        var sellerId = Guid.NewGuid();
        _reviewRepo.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
        var result = await _service.RespondToReviewAsync(review.Id, sellerId, "Thank you");
        result.SellerResponse.Should().Be("Thank you");
        review.RespondedBy.Should().Be(sellerId);
    }

    [Fact]
    public async Task DeleteResponse_ShouldRemoveResponse()
    {
        var review = Review(Guid.NewGuid(), Guid.NewGuid());
        review.AddResponse("Thanks", Guid.NewGuid());
        _reviewRepo.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
        var result = await _service.DeleteResponseAsync(review.Id);
        result.SellerResponse.Should().BeNull();
        review.ResponseDate.Should().BeNull();
    }
}
