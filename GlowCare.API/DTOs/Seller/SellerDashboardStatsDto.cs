namespace GlowCare.API.DTOs.Seller;

/// <summary>Basic sales statistics for the seller dashboard overview.</summary>
public class SellerDashboardStatsDto
{
    public int TotalProducts { get; set; }

    public int ActiveProducts { get; set; }

    public int TotalOrderItems { get; set; }

    public int PendingOrderItems { get; set; }

    /// <summary>Sum of (UnitPrice * Quantity) across this seller's order items.</summary>
    public decimal TotalEarnings { get; set; }

    /// <summary>Sum across items that have actually been fulfilled/paid.</summary>
    public decimal ConfirmedEarnings { get; set; }
}
