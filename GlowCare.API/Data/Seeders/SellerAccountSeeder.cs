using GlowCare.API.Enums;
using GlowCare.API.Models.Identity;
using GlowCare.API.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Data.Seeders;

/// <summary>
/// Creates demo seller accounts with fixed credentials for academic project testing.
/// Passwords are permanent and never regenerated.
/// Accounts are idempotent: existing accounts are not modified.
/// </summary>
public static class SellerAccountSeeder
{
    private static readonly (string Email, string Password, string StoreName, string Description)[] DemoSellers =
    {
        ("seller1@glowcare.local", "GlowCareSeller1!", "GlowCare Store 1", "Premium skincare products"),
        ("seller2@glowcare.local", "GlowCareSeller2!", "GlowCare Store 2", "Beauty essentials"),
        ("seller3@glowcare.local", "GlowCareSeller3!", "GlowCare Store 3", "Derma care specialist"),
        ("seller4@glowcare.local", "GlowCareSeller4!", "GlowCare Store 4", "Natural skincare"),
        ("seller5@glowcare.local", "GlowCareSeller5!", "GlowCare Store 5", "Luxury beauty collection")
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SellerAccountSeeder");

        // Ensure Seller role exists
        if (!await roleManager.RoleExistsAsync("Seller"))
        {
            await roleManager.CreateAsync(new IdentityRole("Seller"));
            logger.LogInformation("Created Seller role");
        }

        foreach (var (email, password, storeName, description) in DemoSellers)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser is null)
            {
                // Create new seller account
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = storeName,
                    EmailConfirmed = true // Demo account - skip email confirmation
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Seller");

                    // Create SellerProfile
                    var profile = new SellerProfile
                    {
                        UserId = user.Id,
                        StoreName = storeName,
                        Description = description,
                        Logo = string.Empty,
                        ApprovalStatus = ApprovalStatus.Approved,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.SellerProfiles.Add(profile);
                    await dbContext.SaveChangesAsync();

                    logger.LogInformation("Created seller account: {Email}", email);
                }
                else
                {
                    logger.LogError(
                        "Failed to create seller account {Email}: {Errors}",
                        email,
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Ensure existing account has Seller role
                if (!await userManager.IsInRoleAsync(existingUser, "Seller"))
                {
                    await userManager.AddToRoleAsync(existingUser, "Seller");
                    logger.LogInformation("Added Seller role to existing account: {Email}", email);
                }

                // Ensure SellerProfile exists and is Approved
                var profile = await dbContext.SellerProfiles
                    .FirstOrDefaultAsync(sp => sp.UserId == existingUser.Id);

                if (profile is null)
                {
                    var newProfile = new SellerProfile
                    {
                        UserId = existingUser.Id,
                        StoreName = storeName,
                        Description = description,
                        Logo = string.Empty,
                        ApprovalStatus = ApprovalStatus.Approved,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.SellerProfiles.Add(newProfile);
                    await dbContext.SaveChangesAsync();

                    logger.LogInformation("Created SellerProfile for existing account: {Email}", email);
                }
                else if (profile.ApprovalStatus != ApprovalStatus.Approved)
                {
                    profile.ApprovalStatus = ApprovalStatus.Approved;
                    await dbContext.SaveChangesAsync();

                    logger.LogInformation("Updated SellerProfile to Approved for: {Email}", email);
                }
                else
                {
                    logger.LogInformation("Seller account already exists with correct profile: {Email}", email);
                }
            }
        }
    }
}
