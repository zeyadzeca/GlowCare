using GlowCare.API.DTOs.Reviews;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// Reviews of ONE product (nested under it) plus its rating aggregate.
/// Writing requires the Customer role AND a verified purchase - both checked
/// in the service. Reviews themselves are edited/removed via ReviewsController.
/// </summary>
[ApiController]
[Route("api/products/{productId:int}/reviews")]
public class ProductReviewsController : ControllerBase
{
    private readonly IReviewService _reviews;

    public ProductReviewsController(IReviewService reviews)
    {
        _reviews = reviews;
    }

    /// <summary>All reviews of the product, newest first.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IList<ReviewDto>>> GetReviews(int productId)
        => Ok(await _reviews.GetProductReviewsAsync(productId));

    /// <summary>Average rating + review count (used by product cards/lists).</summary>
    [HttpGet("/api/products/{productId:int}/rating")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewSummaryDto>> GetRating(int productId)
        => Ok(await _reviews.GetProductRatingAsync(productId));

    /// <summary>Adds MY review - only for products I purchased and received.</summary>
    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> AddReview(int productId, ReviewCreateDto dto)
    {
        var review = await _reviews.AddReviewAsync(User.GetUserId()!, productId, dto);
        return CreatedAtAction(nameof(GetReviews), new { productId }, review);
    }
}
