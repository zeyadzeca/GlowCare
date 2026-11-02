using GlowCare.API.DTOs.Admin;
using GlowCare.API.DTOs.Seller;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>Admin dashboard: stats, users/roles and seller approvals.</summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin)
    {
        _admin = admin;
    }

    /// <summary>Top-line platform numbers (users, orders, revenue, applications).</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboard()
        => Ok(await _admin.GetDashboardStatsAsync());

    /// <summary>All user accounts with their roles and lockout state.</summary>
    [HttpGet("users")]
    public async Task<ActionResult<IList<UserManagementDto>>> GetUsers([FromQuery] string? search = null)
        => Ok(await _admin.GetUsersAsync(search));

    /// <summary>Update user name, email, or lockout status.</summary>
    [HttpPut("users/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, UpdateUserDto dto)
    {
        if (!await _admin.UpdateUserAsync(userId, dto))
        {
            return NotFound(new { statusCode = 404, message = $"User '{userId}' was not found." });
        }

        return Ok(new { message = "User updated." });
    }

    /// <summary>Grants or removes a role (Admin, Seller, Customer).</summary>
    [HttpPut("users/{userId}/role")]
    public async Task<IActionResult> UpdateUserRole(string userId, UpdateUserRoleDto dto)
    {
        // Safety: an admin must never be able to demote THEMSELVES out of the
        // Admin role (they would immediately lose access to fix it).
        if (!dto.Assign &&
            string.Equals(userId, User.GetUserId(), StringComparison.Ordinal) &&
            string.Equals(dto.Role.Trim(), "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                statusCode = 400,
                message = "You cannot remove your own Admin role."
            });
        }

        if (!await _admin.UpdateUserRoleAsync(userId, dto))
        {
            return NotFound(new { statusCode = 404, message = $"User '{userId}' was not found." });
        }

        return Ok(new { message = "Role updated." });
    }

    /// <summary>All seller applications (pending, approved, rejected), newest first.</summary>
    [HttpGet("seller-applications")]
    public async Task<ActionResult<IList<SellerApplicationListDto>>> GetSellerApplications()
        => Ok(await _admin.GetSellerApplicationsAsync());

    /// <summary>
    /// Approves or rejects one application. Approving keeps the Seller role;
    /// rejecting revokes it so a rejected applicant loses seller access.
    /// </summary>
    [HttpPut("seller-applications/{sellerProfileId:int}/approval")]
    [HttpPut("sellers/{sellerProfileId:int}/approval")]
    public async Task<ActionResult<SellerApplicationListDto>> UpdateSellerApproval(int sellerProfileId, SellerApprovalDto dto)
    {
        var application = await _admin.UpdateSellerApprovalAsync(sellerProfileId, dto.IsApproved);

        return application is null
            ? NotFound(new { statusCode = 404, message = $"Application {sellerProfileId} was not found." })
            : Ok(application);
    }
}
