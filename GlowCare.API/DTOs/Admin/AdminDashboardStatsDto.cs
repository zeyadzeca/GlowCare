namespace GlowCare.API.DTOs.Admin;

/// <summary>Top-line numbers for the admin dashboard overview.</summary>
public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }

    public int TotalSellers { get; set; }

    public int PendingSellerApplications { get; set; }

    public int TotalProducts { get; set; }

    public int ActiveProducts { get; set; }

    public int TotalOrders { get; set; }

    public int PendingOrders { get; set; }

    /// <summary>Sum of TotalAmount across all orders (all statuses).</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>Sum across orders that are Delivered and Paid.</summary>
    public decimal ConfirmedRevenue { get; set; }
}
