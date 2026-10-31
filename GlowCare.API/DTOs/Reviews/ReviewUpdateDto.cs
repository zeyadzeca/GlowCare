using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Reviews;

/// <summary>Request to edit an existing review (author only).</summary>
public class ReviewUpdateDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
