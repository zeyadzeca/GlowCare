using GlowCare.API.DTOs.Admin;
using GlowCare.API.DTOs.Seller;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Admin dashboard: statistics, user/role management and seller approvals.
/// All methods require the Admin role (enforced by the controller attribute).
/// </summary>
public interface IAdminService
{
    /// <summary>Top-line platform statistics for the admin dashboard.</summary>
    Task<AdminDashboardStatsDto> GetDashboardStatsAsync();

    /// <summary>All user accounts with their roles. Optional search by name or email.</summary>
    Task<IList<UserManagementDto>> GetUsersAsync(string? search = null);

    /// <summary>Update user name, email, or lockout status. Returns false if user not found.</summary>
    Task<bool> UpdateUserAsync(string userId, UpdateUserDto dto);

    /// <summary>Grants or removes a role. Returns false if the user does not exist.</summary>
    Task<bool> UpdateUserRoleAsync(string userId, UpdateUserRoleDto dto);

    /// <summary>All seller applications (pending, approved and rejected), newest first.</summary>
    Task<IList<SellerApplicationListDto>> GetSellerApplicationsAsync();

    /// <summary>
    /// Approves or rejects one seller application by SellerProfileId.
    /// Approving also grants the Seller role; rejecting removes it.
    /// Returns null if the application does not exist.
    /// </summary>
    Task<SellerApplicationListDto?> UpdateSellerApprovalAsync(int sellerProfileId, bool isApproved);
}
