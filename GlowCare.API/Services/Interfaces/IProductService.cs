using GlowCare.API.DTOs.Common;
using GlowCare.API.DTOs.Products;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Public product catalog (guests allowed) + seller product management.
/// Only ACTIVE products are visible publicly. Only the owning approved seller
/// can create/edit/deactivate their own products (sellerId from JWT claims).
/// </summary>
public interface IProductService
{
    /// <summary>Paged, filtered, sorted list of ACTIVE products (public).</summary>
    Task<PagedResultDto<ProductListDto>> GetProductsAsync(ProductFilterDto filter);

    /// <summary>Full details of one product by id (public; inactive only visible to its seller/admin - handled by caller).</summary>
    Task<ProductResponseDto?> GetProductByIdAsync(int id);

    /// <summary>This seller's products (active and inactive), newest first.</summary>
    Task<IList<ProductResponseDto>> GetSellerProductsAsync(string sellerId);

    /// <summary>Creates a product for the authenticated approved seller.</summary>
    Task<ProductResponseDto> CreateProductAsync(string sellerId, ProductCreateDto dto);

    /// <summary>Updates a product the seller owns. Returns null if not found or not owned.</summary>
    Task<ProductResponseDto?> UpdateProductAsync(string sellerId, int productId, ProductUpdateDto dto);

    /// <summary>Soft-deletes (deactivates) a product the seller owns.</summary>
    Task<bool> DeactivateProductAsync(string sellerId, int productId);

    /// <summary>Reactivates a product the seller owns.</summary>
    Task<bool> ActivateProductAsync(string sellerId, int productId);

    /// <summary>Permanently deletes a product (Admin only). Fails if order items reference it.</summary>
    Task<bool> AdminDeleteProductAsync(int productId);

    /// <summary>Admin creates a product (can assign to any seller).</summary>
    Task<ProductResponseDto> AdminCreateProductAsync(ProductCreateDto dto);

    /// <summary>Admin updates any product.</summary>
    Task<ProductResponseDto?> AdminUpdateProductAsync(int productId, ProductUpdateDto dto);
}
