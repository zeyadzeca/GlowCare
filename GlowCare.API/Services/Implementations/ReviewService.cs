using GlowCare.API.Data;
using GlowCare.API.DTOs.Reviews;
using GlowCare.API.Enums;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Reviews;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Product reviews and ratings.
///
/// RULES enforced here:
/// - Only customers whose Delivered (or Paid) order contained the product may
///   review it ("verified purchaser").
/// - One review per customer per product (also backed by a unique index).
/// - Rating 1..5 (also backed by a database check constraint).
/// </summary>
public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IList<ReviewDto>> GetProductReviewsAsync(int productId)
        => await _db.Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                UserName = r.User.FullName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

    public async Task<ReviewSummaryDto> GetProductRatingAsync(int productId)
    {
        var stats = await _db.Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Average = g.Average(r => (double)r.Rating) })
            .FirstOrDefaultAsync();

        return new ReviewSummaryDto
        {
            ReviewCount = stats?.Count ?? 0,
            AverageRating = stats?.Average ?? 0
        };
    }

    public async Task<ReviewDto> AddReviewAsync(string userId, int productId, ReviewCreateDto dto)
    {
        if (!await _db.Products.AsNoTracking().AnyAsync(p => p.Id == productId))
        {
            throw new AppException($"Product {productId} was not found.", 404);
        }

        // Verified purchaser: the product must appear in one of MY orders that
        // is Delivered (or already Paid).
        var verifiedPurchase = await _db.OrderItems.AsNoTracking()
            .AnyAsync(oi => oi.ProductId == productId &&
                            oi.Order.UserId == userId &&
                            (oi.Order.Status == OrderStatus.Delivered ||
                             oi.Order.PaymentStatus == PaymentStatus.Paid));

        if (!verifiedPurchase)
        {
            throw new AppException(
                "You can only review products you have purchased and received.",
                403);
        }

        var alreadyReviewed = await _db.Reviews.AsNoTracking()
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId);

        if (alreadyReviewed)
        {
            throw new AppException("You have already reviewed this product.", 409);
        }

        var review = new Review
        {
            UserId = userId,
            ProductId = productId,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var fullName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.FullName)
            .FirstAsync();

        return Map(review, fullName);
    }

    public async Task<ReviewDto?> UpdateReviewAsync(string userId, int reviewId, ReviewUpdateDto dto)
    {
        var review = await _db.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review is null || review.UserId != userId)
        {
            return null; // not found or not yours
        }

        review.Rating = dto.Rating;
        review.Comment = dto.Comment.Trim();
        await _db.SaveChangesAsync();

        return Map(review, review.User.FullName);
    }

    public async Task<bool> DeleteReviewAsync(string userId, int reviewId)
    {
        var review = await _db.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

        if (review is null)
        {
            return false;
        }

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ReviewDto Map(Review review, string fullName) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        UserId = review.UserId,
        UserName = fullName,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };
}
