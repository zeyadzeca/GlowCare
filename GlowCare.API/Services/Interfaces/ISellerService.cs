using GlowCare.API.DTOs.Seller;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// The seller dashboard: statistics and order-item fulfillment.
/// EVERY method is scoped to the authenticated seller (sellerId from JWT
/// claims) - a seller can never see another seller's data.
/// </summary>
public interface ISellerService
{
    /// <summary>Product counts and earnings for this seller's dashboard overview.</summary>
    Task<SellerDashboardStatsDto> GetDashboardStatsAsync(string sellerId);

    /// <summary>This seller's order items (from all their orders), newest first.</summary>
    Task<IList<SellerOrderItemDto>> GetOrderItemsAsync(string sellerId);

    /// <summary>
    /// Marks one of MY order items as fulfilled. Returns null if the item does
    /// not belong to this seller.
    /// </summary>
    Task<SellerOrderItemDto?> UpdateFulfillmentAsync(string sellerId, int orderItemId, bool fulfilled);
}
