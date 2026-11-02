using GlowCare.API.DTOs.Common;
using GlowCare.API.DTOs.Products;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// Product catalog:
/// - GET endpoints are public (guests can browse).
/// - Create/update/deactivate are for the owning approved SELLER - the seller
///   id always comes from JWT claims, never from the request body.
/// - Permanent delete is Admin only (and refused when order items reference it).
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;

    public ProductsController(IProductService products)
    {
        _products = products;
    }

    // ------------------------------------------------------------------
    // Public
    // ------------------------------------------------------------------

    /// <summary>Paged, filtered, sorted list of ACTIVE products.</summary>
    /// <example>GET /api/products?brandId=1&amp;minPrice=100&amp;sortBy=price_asc&amp;page=1&amp;pageSize=10</example>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResultDto<ProductListDto>>> GetProducts([FromQuery] ProductFilterDto filter)
        => Ok(await _products.GetProductsAsync(filter));

    /// <summary>Full details of one product (404 for unknown or inactive products).</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _products.GetProductByIdAsync(id);

        if (product is null)
        {
            return NotFound(new { statusCode = 404, message = $"Product {id} was not found." });
        }

        // Inactive (deactivated) products are only visible to their own seller and admins.
        if (!product.IsActive)
        {
            var callerId = User.GetUserId();
            var isOwner = callerId is not null && callerId == product.SellerId;
            if (!isOwner && !User.IsInRole("Admin"))
            {
                return NotFound(new { statusCode = 404, message = $"Product {id} was not found." });
            }
        }

        return Ok(product);
    }

    // ------------------------------------------------------------------
    // Seller (owner) - approved sellers only, enforced inside the service
    // ------------------------------------------------------------------

    /// <summary>Creates a product for the authenticated approved seller.</summary>
    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(ProductCreateDto dto)
    {
        var created = await _products.CreateProductAsync(User.GetUserId()!, dto);
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }

    /// <summary>Updates a product the caller owns (404 otherwise).</summary>
    [Authorize(Roles = "Seller")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> UpdateProduct(int id, ProductUpdateDto dto)
    {
        var updated = await _products.UpdateProductAsync(User.GetUserId()!, id, dto);

        if (updated is null)
        {
            return NotFound(new { statusCode = 404, message = $"Product {id} was not found or is not yours." });
        }

        return Ok(updated);
    }

    /// <summary>Soft-delete: deactivates the product (it disappears from the public catalog).</summary>
    [Authorize(Roles = "Seller")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateProduct(int id)
    {
        if (!await _products.DeactivateProductAsync(User.GetUserId()!, id))
        {
            return NotFound(new { statusCode = 404, message = $"Product {id} was not found or is not yours." });
        }

        return Ok(new { message = "Product deactivated." });
    }

    /// <summary>Reactivates a previously deactivated product.</summary>
    [Authorize(Roles = "Seller")]
    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> ActivateProduct(int id)
    {
        if (!await _products.ActivateProductAsync(User.GetUserId()!, id))
        {
            return NotFound(new { statusCode = 404, message = $"Product {id} was not found or is not yours." });
        }

        return Ok(new { message = "Product activated." });
    }

    // ------------------------------------------------------------------
    // Admin
    // ------------------------------------------------------------------

    /// <summary>Admin creates a product (can assign to any seller).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin")]
    public async Task<ActionResult<ProductResponseDto>> AdminCreateProduct(ProductCreateDto dto)
    {
        var created = await _products.AdminCreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }

    /// <summary>Admin updates any product.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("admin/{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> AdminUpdateProduct(int id, ProductUpdateDto dto)
    {
        var updated = await _products.AdminUpdateProductAsync(id, dto);
        if (updated is null)
        {
            return NotFound(new { statusCode = 404, message = $"Product {id} was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Permanently deletes a product (Admin only). Fails with 409 while any
    /// order item references it - purchase history can never be destroyed.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}/permanent")]
    public async Task<IActionResult> AdminDeleteProduct(int id)
    {
        var exists = await _products.GetProductByIdAsync(id);

        if (exists is null)
        {
            return NotFound(new { statusCode = 404, message = $"Product {id} was not found." });
        }

        if (!await _products.AdminDeleteProductAsync(id))
        {
            return Conflict(new
            {
                statusCode = 409,
                message = "This product cannot be deleted because order items reference it."
            });
        }

        return Ok(new { message = "Product permanently deleted." });
    }
}
