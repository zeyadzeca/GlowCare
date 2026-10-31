using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Seller;

/// <summary>Marks one of the seller's own order items as fulfilled or not.</summary>
public class FulfillmentUpdateDto
{
    [Required]
    public bool Fulfilled { get; set; }
}
