using GlowCare.API.Data;
using GlowCare.API.DTOs.Admin;
using GlowCare.API.DTOs.Seller;
using GlowCare.API.Enums;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Identity;
using GlowCare.API.Models.User;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Admin dashboard: platform statistics, user/role management and seller
/// approvals. Role changes go through ASP.NET Identity (never a hand-written
/// UPDATE), and approving/rejecting keeps the Seller role in sync.
/// </summary>
public class AdminService : IAdminService
{
    private static readonly string[] ValidRoles = { "Admin", "Seller", "Customer" };

    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
    {
        var orders = _db.Orders.AsNoTracking();

        return new AdminDashboardStatsDto
        {
            TotalUsers = await _db.Users.AsNoTracking().CountAsync(),
            TotalSellers = await _db.SellerProfiles.AsNoTracking()
                .CountAsync(s => s.ApprovalStatus == ApprovalStatus.Approved),
            PendingSellerApplications = await _db.SellerProfiles.AsNoTracking()
                .CountAsync(s => s.ApprovalStatus == ApprovalStatus.Pending),
            TotalProducts = await _db.Products.AsNoTracking().CountAsync(),
            ActiveProducts = await _db.Products.AsNoTracking().CountAsync(p => p.IsActive),
            TotalOrders = await orders.CountAsync(),
            PendingOrders = await orders.CountAsync(o => o.Status == OrderStatus.Pending),
            TotalRevenue = await orders.SumAsync(o => o.TotalAmount),
            // Confirmed = actually delivered AND actually paid (spec definition).
            ConfirmedRevenue = await orders
                .Where(o => o.Status == OrderStatus.Delivered && o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount)
        };
    }

    public async Task<IList<UserManagementDto>> GetUsersAsync(string? search = null)
    {
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(searchTerm) || (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
        }

        var users = await query.OrderBy(u => u.Email).ToListAsync();

        var result = new List<UserManagementDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(new UserManagementDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                IsLockedOut = user.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        return result;
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim();

        if (dto.IsLockedOut)
        {
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }
        else
        {
            user.LockoutEnd = null;
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new AppException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(string userId, UpdateUserRoleDto dto)
    {
        var role = dto.Role.Trim();

        if (!ValidRoles.Contains(role))
        {
            throw new AppException(
                $"Invalid role '{role}'. Valid roles: {string.Join(", ", ValidRoles)}.", 400);
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        if (dto.Assign)
        {
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.AddToRoleAsync(user, role);

                if (!result.Succeeded)
                {
                    throw new AppException(string.Join(" ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        else
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role);

                if (!result.Succeeded)
                {
                    throw new AppException(string.Join(" ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        return true;
    }

    public async Task<IList<SellerApplicationListDto>> GetSellerApplicationsAsync()
    {
        var profiles = await _db.SellerProfiles.AsNoTracking()
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return profiles.Select(Map).ToList();
    }

    public async Task<SellerApplicationListDto?> UpdateSellerApprovalAsync(int sellerProfileId, bool isApproved)
    {
        var profile = await _db.SellerProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sellerProfileId);

        if (profile is null)
        {
            return null;
        }

        profile.ApprovalStatus = isApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;

        // The Seller role is granted when the application is submitted, so it
        // must be revoked on rejection - a rejected seller loses all seller access.
        if (isApproved)
        {
            if (!await _userManager.IsInRoleAsync(profile.User, "Seller"))
            {
                await _userManager.AddToRoleAsync(profile.User, "Seller");
            }
        }
        else
        {
            if (await _userManager.IsInRoleAsync(profile.User, "Seller"))
            {
                await _userManager.RemoveFromRoleAsync(profile.User, "Seller");
            }
        }

        await _db.SaveChangesAsync();

        return Map(profile);
    }

    private static SellerApplicationListDto Map(SellerProfile profile) => new()
    {
        SellerProfileId = profile.Id,
        UserId = profile.UserId,
        FullName = profile.User.FullName,
        Email = profile.User.Email ?? string.Empty,
        StoreName = profile.StoreName,
        Description = profile.Description,
        Logo = profile.Logo,
        ApprovalStatus = profile.ApprovalStatus,
        CreatedAt = profile.CreatedAt
    };
}
