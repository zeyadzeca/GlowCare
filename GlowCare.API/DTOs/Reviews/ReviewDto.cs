namespace GlowCare.API.DTOs.Reviews;

/// <summary>A product review with the reviewer's display name.</summary>
public class ReviewDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
