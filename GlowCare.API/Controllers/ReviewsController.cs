using GlowCare.API.DTOs.Reviews;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// Editing and deleting MY OWN reviews (addressed by the review's own id).
/// Listing/creating reviews happens under /api/products/{id}/reviews.
/// </summary>
[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviews;

    public ReviewsController(IReviewService reviews)
    {
        _reviews = reviews;
    }

    /// <summary>Updates my review (404 if it does not exist or is not mine).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(int id, ReviewUpdateDto dto)
    {
        var review = await _reviews.UpdateReviewAsync(User.GetUserId()!, id, dto);

        return review is null
            ? NotFound(new { statusCode = 404, message = $"Review {id} was not found." })
            : Ok(review);
    }

    /// <summary>Deletes my review.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        if (!await _reviews.DeleteReviewAsync(User.GetUserId()!, id))
        {
            return NotFound(new { statusCode = 404, message = $"Review {id} was not found." });
        }

        return Ok(new { message = "Review deleted." });
    }

    // ------------------------------------------------------------------
    // Admin
    // ------------------------------------------------------------------

    /// <summary>All reviews (Admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IList<ReviewDto>>> GetAllReviews()
        => Ok(await _reviews.GetAllReviewsAsync());

    /// <summary>Admin deletes any review.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("admin/{id:int}")]
    public async Task<IActionResult> AdminDeleteReview(int id)
    {
        if (!await _reviews.AdminDeleteReviewAsync(id))
        {
            return NotFound(new { statusCode = 404, message = $"Review {id} was not found." });
        }
        return Ok(new { message = "Review deleted." });
    }
}
