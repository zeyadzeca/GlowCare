using GlowCare.API.DTOs.Reviews;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Product reviews and ratings.
/// RULES:
/// - Only customers whose DELIVERED/PAID order contained the product may review it.
/// - One review per customer per product (enforced here and by a unique index).
/// - Rating 1..5 (enforced here and by a check constraint).
/// </summary>
public interface IReviewService
{
    /// <summary>All reviews for a product, newest first (public).</summary>
    Task<IList<ReviewDto>> GetProductReviewsAsync(int productId);

    /// <summary>Average rating + count for a product (public).</summary>
    Task<ReviewSummaryDto> GetProductRatingAsync(int productId);

    /// <summary>Adds the authenticated customer's review. Throws if not a verified purchaser or already reviewed.</summary>
    Task<ReviewDto> AddReviewAsync(string userId, int productId, ReviewCreateDto dto);

    /// <summary>Edits the user's own review. Returns null if not found or not the author.</summary>
    Task<ReviewDto?> UpdateReviewAsync(string userId, int reviewId, ReviewUpdateDto dto);

    /// <summary>Deletes the user's own review. Returns false if not found or not the author.</summary>
    Task<bool> DeleteReviewAsync(string userId, int reviewId);
}
