using Microsoft.AspNetCore.Http;
using AutoMapper;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.DTOs.Reviews;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository,
        IFileStorageService fileStorage, IMapper mapper, ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _fileStorage = fileStorage;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid productId, Guid userId, int rating, string comment,
        IFormFileCollection? images, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct);
        if (product == null) throw new NotFoundException("Product", productId);

        var ratingObj = new Rating(rating);
        var review = new Review(productId, userId, ratingObj, comment);

        if (images != null && images.Count > 0)
        {
            var urls = new List<string>();
            foreach (var file in images)
            {
                if (file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    var url = await _fileStorage.UploadAsync(stream, file.FileName, file.ContentType, ct);
                    urls.Add(url);
                }
            }
            review.SetImageUrls(urls);
        }

        await _reviewRepository.AddAsync(review, ct);
        await _reviewRepository.SaveChangesAsync(ct);
        return _mapper.Map<ReviewDto>(review);
    }

    public async Task<IReadOnlyList<ReviewDto>> GetProductReviewsAsync(Guid productId, CancellationToken ct = default)
    {
        var (reviews, _) = await _reviewRepository.GetProductReviewsAsync(productId, 1, 100, ct);
        return _mapper.Map<List<ReviewDto>>(reviews);
    }

    public async Task<ReviewDto> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewRequest request, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, ct);
        if (review == null) throw new NotFoundException("Review", reviewId);
        if (review.UserId != userId) throw new ForbiddenException("You can only edit your own reviews");
        if (request.Rating.HasValue) review.SetRating(new Rating(request.Rating.Value));
        if (request.Comment != null) review.SetComment(request.Comment);
        await _reviewRepository.UpdateAsync(review, ct);
        await _reviewRepository.SaveChangesAsync(ct);
        return _mapper.Map<ReviewDto>(review);
    }

    public async Task DeleteReviewAsync(Guid reviewId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, ct);
        if (review == null) throw new NotFoundException("Review", reviewId);
        if (!isAdmin && review.UserId != userId)
            throw new ForbiddenException("You can only delete your own reviews.");
        await _reviewRepository.DeleteAsync(review, ct);
        await _reviewRepository.SaveChangesAsync(ct);
    }


    public async Task<ReviewDto> RespondToReviewAsync(Guid reviewId, Guid userId, string response, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, ct);
        if (review == null) throw new NotFoundException("Review", reviewId);
        review.AddResponse(response, userId);
        await _reviewRepository.UpdateAsync(review, ct);
        await _reviewRepository.SaveChangesAsync(ct);
        return _mapper.Map<ReviewDto>(review);
    }

    public async Task<ReviewDto> DeleteResponseAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, ct);
        if (review == null) throw new NotFoundException("Review", reviewId);
        review.RemoveResponse();
        await _reviewRepository.UpdateAsync(review, ct);
        await _reviewRepository.SaveChangesAsync(ct);
        return _mapper.Map<ReviewDto>(review);
    }
}
