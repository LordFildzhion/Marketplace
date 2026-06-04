using System.Security.Claims;
using Marketplace.Application.DTOs.Reviews;
using Marketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Web.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService) => _reviewService = reviewService;

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetProductReviews(Guid productId)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId);
        return Ok(reviews);
    }

    [HttpPost("product/{productId:guid}")]
    [Authorize]
    [RequestSizeLimit(10_000_000)] // до 10 МБ
    public async Task<ActionResult<ReviewDto>> CreateReview(Guid productId,
        [FromForm] int rating,
        [FromForm] string comment,
        IFormFileCollection? images)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var review = await _reviewService.CreateReviewAsync(productId, userId, rating, comment, images);
        return CreatedAtAction(nameof(GetProductReviews), new { productId }, review);
    }

    [HttpPut("{reviewId:guid}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid reviewId, UpdateReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var review = await _reviewService.UpdateReviewAsync(reviewId, userId, request);
        return Ok(review);
    }

    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteReview(Guid reviewId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");
        await _reviewService.DeleteReviewAsync(reviewId, userId, isAdmin);
        return NoContent();
    }


    [HttpPost("{reviewId:guid}/response")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<ReviewDto>> RespondToReview(Guid reviewId, RespondToReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var review = await _reviewService.RespondToReviewAsync(reviewId, userId, request.Response);
        return Ok(review);
    }

    [HttpDelete("{reviewId:guid}/response")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ReviewDto>> DeleteResponse(Guid reviewId)
    {
        var review = await _reviewService.DeleteResponseAsync(reviewId);
        return Ok(review);
    }
}
