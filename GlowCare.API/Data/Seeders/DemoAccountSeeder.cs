using GlowCare.API.Data;
using GlowCare.API.Enums;
using GlowCare.API.Models.Identity;
using GlowCare.API.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Data.Seeders;

/// <summary>
/// Creates fixed demo accounts for local/academic demonstrations.
/// Idempotent: existing passwords are never reset or overwritten.
/// Do not enable this seeder in production.
/// </summary>
public static class DemoAccountSeeder
{
    private sealed record DemoSeller(string UserName, string Email, string Password,
        string StoreName, string Description);
    private sealed record DemoAdmin(string UserName, string Email, string Password, string FullName);

    private static readonly DemoSeller[] Sellers =
    {
        new("glowessentials", "glowessentials@glowcare.local", "Gl0w!Ess_82#Care", "Glow Essentials", "Premium skincare products"),
        new("pureskin", "pureskin@glowcare.local", "Pure#Skin_49!Glow", "Pure Skin Store", "Everyday skincare essentials"),
        new("dermahouse", "dermahouse@glowcare.local", "Derma!House_73#S", "Derma House", "Dermatology-inspired skincare"),
        new("skinparadise", "skinparadise@glowcare.local", "Paradise#Skin_61!", "Skin Paradise", "Skincare and beauty essentials"),
        new("beautybloom", "beautybloom@glowcare.local", "Bloom!Beauty_94#K", "Beauty Bloom", "Beauty and skincare collection"),
        new("skincarehub", "skincarehub@glowcare.local", "Hub#Care_57!Skin", "The Skincare Hub", "A hub for daily skincare"),
        new("radianttouch", "radianttouch@glowcare.local", "Radiant!Touch_38#", "Radiant Touch", "Products for a radiant routine"),
        new("purederma", "purederma@glowcare.local", "Pure!Derma_65#Glow", "Pure Derma", "Focused derma care"),
        new("velvetskin", "velvetskin@glowcare.local", "Velvet#Skin_29!Care", "Velvet Skin", "Premium skin essentials"),
        new("skinsecrets", "skinsecrets@glowcare.local", "Secrets!Skin_84#G", "Skin Secrets", "Curated skincare products")
    };

    private static readonly DemoAdmin[] Admins =
    {
        new("admin01", "admin01@glowcare.local", "Admin!Glow_81#One", "GlowCare Admin 01"),
        new("admin02", "admin02@glowcare.local", "Admin!Glow_72#Two", "GlowCare Admin 02"),
        new("admin03", "admin03@glowcare.local", "Admin!Glow_63#Three", "GlowCare Admin 03"),
        new("admin04", "admin04@glowcare.local", "Admin!Glow_54#Four", "GlowCare Admin 04"),
        new("admin05", "admin05@glowcare.local", "Admin!Glow_45#Five", "GlowCare Admin 05")
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DemoAccountSeeder");

        foreach (var role in new[] { "Admin", "Seller", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Could not create role {role}: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        foreach (var seller in Sellers)
        {
            var user = await userManager.FindByEmailAsync(seller.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = seller.UserName,
                    Email = seller.Email,
                    FullName = seller.StoreName,
                    EmailConfirmed = true
                };
                var create = await userManager.CreateAsync(user, seller.Password);
                if (!create.Succeeded)
                    throw new InvalidOperationException($"Could not create {seller.Email}: {string.Join("; ", create.Errors.Select(e => e.Description))}");
            }
            else if (!await userManager.IsEmailConfirmedAsync(user))
            {
                user.EmailConfirmed = true;
                var update = await userManager.UpdateAsync(user);
                if (!update.Succeeded)
                    throw new InvalidOperationException($"Could not update {seller.Email}: {string.Join("; ", update.Errors.Select(e => e.Description))}");
            }

            if (!await userManager.IsInRoleAsync(user, "Seller"))
            {
                var roleResult = await userManager.AddToRoleAsync(user, "Seller");
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Could not assign Seller role to {seller.Email}: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }

            var profile = await db.SellerProfiles.SingleOrDefaultAsync(x => x.UserId == user.Id);
            if (profile is null)
            {
                db.SellerProfiles.Add(new SellerProfile
                {
                    UserId = user.Id,
                    StoreName = seller.StoreName,
                    Description = seller.Description,
                    Logo = string.Empty,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            else
            {
                // Keep the existing profile data; only ensure demo sellers are approved.
                if (profile.ApprovalStatus != ApprovalStatus.Approved)
                {
                    profile.ApprovalStatus = ApprovalStatus.Approved;
                    await db.SaveChangesAsync();
                }
            }
        }

        foreach (var admin in Admins)
        {
            var user = await userManager.FindByEmailAsync(admin.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = admin.UserName,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    EmailConfirmed = true
                };
                var create = await userManager.CreateAsync(user, admin.Password);
                if (!create.Succeeded)
                    throw new InvalidOperationException($"Could not create {admin.Email}: {string.Join("; ", create.Errors.Select(e => e.Description))}");
            }

            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Could not assign Admin role to {admin.Email}: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        logger.LogInformation("Demo account seeding completed: {SellerCount} sellers, {AdminCount} admins.", Sellers.Length, Admins.Length);
    }
}
