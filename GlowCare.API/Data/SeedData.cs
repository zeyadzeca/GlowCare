using GlowCare.API.Helpers;
using GlowCare.API.Models.Catalog;
using GlowCare.API.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GlowCare.API.Data;

/// <summary>
/// Runs at application startup:
/// 1. Ensures the Admin, Seller and Customer roles exist (role seeding).
/// 2. Creates the initial Admin account from secure configuration (if missing).
///
/// The admin password is read from User Secrets / environment variables -
/// it is never hardcoded in source code.
/// </summary>
public static class SeedData
{
    public static readonly string[] Roles = { "Admin", "Seller", "Customer" };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var adminSettings = scope.ServiceProvider.GetRequiredService<IOptions<AdminSettings>>().Value;
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SeedData");

        // --------------------------------------------------
        // 1. Role seeding
        // --------------------------------------------------
        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Created role: {Role}", roleName);
            }
        }

        // --------------------------------------------------
        // 2. Skin type seeding (the 5 standard values)
        // --------------------------------------------------
        var standardSkinTypes = new[] { "Oily", "Dry", "Combination", "Sensitive", "Normal" };
        var existingSkinTypes = await dbContext.SkinTypes.Select(s => s.Name).ToListAsync();

        foreach (var skinTypeName in standardSkinTypes)
        {
            if (!existingSkinTypes.Contains(skinTypeName))
            {
                dbContext.SkinTypes.Add(new SkinType { Name = skinTypeName });
                logger.LogInformation("Created skin type: {SkinType}", skinTypeName);
            }
        }
        await dbContext.SaveChangesAsync();

        // --------------------------------------------------
        // 3. Initial Admin account from configuration
        // --------------------------------------------------
        if (string.IsNullOrWhiteSpace(adminSettings.Email) ||
            string.IsNullOrWhiteSpace(adminSettings.Password))
        {
            logger.LogWarning(
                "Initial admin account not created: Admin:Email or Admin:Password is not configured. " +
                "Set them with: dotnet user-secrets set \"Admin:Password\" \"<value>\"");
            return;
        }

        var existingAdmin = await userManager.Users
            .FirstOrDefaultAsync(u => u.Email == adminSettings.Email);

        if (existingAdmin is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminSettings.Email,
                Email = adminSettings.Email,
                FullName = adminSettings.FullName,
                EmailConfirmed = true // seeded admin - no email confirmation flow in this project
            };

            var result = await userManager.CreateAsync(admin, adminSettings.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Created initial admin account: {Email}", adminSettings.Email);
            }
            else
            {
                logger.LogError(
                    "Failed to create admin account: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Make sure the existing account keeps the Admin role.
            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
            }

            logger.LogInformation("Admin account already exists: {Email}", adminSettings.Email);
        }
    }
}
