namespace GlowCare.API.DTOs.Reviews;

/// <summary>Aggregated rating of a product, shown on cards and the details page.</summary>
public class ReviewSummaryDto
{
    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}
