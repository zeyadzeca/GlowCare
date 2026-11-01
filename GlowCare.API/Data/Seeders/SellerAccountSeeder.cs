using GlowCare.API.Enums;
using GlowCare.API.Models.Identity;
using GlowCare.API.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Data.Seeders;

/// <summary>
/// Seeds the five fixed academic demo seller accounts.
///
/// These are PERMANENT demo credentials for a local academic project: the
/// passwords are deliberate constants so the team can log in on every machine,
/// not temporary/random values. Because of that this seeder is Development-only
/// (it is only invoked from the Development startup block in Program.cs) and the
/// passwords are never written to logs or configuration files.
///
/// Idempotency rules - safe to run on every startup:
///   * Creates only accounts whose email does not already exist.
///   * NEVER changes the password of an existing account. If an existing account
///     does not match the documented password we report that its password was
///     left unchanged instead of claiming the documented credentials work.
///   * Ensures the Seller role exists and is assigned.
///   * Creates a SellerProfile only when the user has none; existing profiles
///     (including their ApprovalStatus) are left untouched.
///   * Does not touch unrelated users, roles or seller profiles.
/// </summary>
public static class SellerAccountSeeder
{
    /// <summary>
    /// The fixed demo credentials. Documented for the academic project.
    /// Passwords are only ever handed to Identity's password hasher.
    /// </summary>
    private static readonly (string Email, string Password, string StoreName, string Description)[] Sellers =
    {
        ("seller1@glowcare.local", "GlowCareSeller1!", "Aurora Skin Lab",
            "Serums and ampoules focused on barrier repair and glow."),
        ("seller2@glowcare.local", "GlowCareSeller2!", "Pure Dew Cosmetics",
            "Gentle daily hydration for sensitive and dry skin."),
        ("seller3@glowcare.local", "GlowCareSeller3!", "Botanica Glow",
            "Plant-derived actives with transparent ingredient lists."),
        ("seller4@glowcare.local", "GlowCareSeller4!", "Solar Shield Store",
            "Sunscreen and SPF essentials for every skin type."),
        ("seller5@glowcare.local", "GlowCareSeller5!", "Night Bloom Beauty",
            "Overnight treatments, retinol and renewing formulas."),
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

        // 1. Guarantee the Seller role exists (SeedData normally creates it first).
        if (!await roleManager.RoleExistsAsync("Seller"))
        {
            await roleManager.CreateAsync(new IdentityRole("Seller"));
            logger.LogInformation("Created role: Seller");
        }

        foreach (var (email, password, storeName, description) in Sellers)
        {
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                // ----------------------------------------------------------
                // Create the account. Identity validates the password against
                // the configured rules and hashes it - we never hash manually.
                // ----------------------------------------------------------
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = storeName,
                    EmailConfirmed = true // local academic demo account
                };

                var createResult = await userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    logger.LogError(
                        "Failed to create demo seller {Email}: {Errors}",
                        email,
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));
                    continue;
                }

                logger.LogInformation("Created demo seller account: {Email}", email);
            }
            else
            {
                // Account already exists: report (do not silently claim the
                // documented password works) and leave the password untouched.
                if (await userManager.CheckPasswordAsync(user, password))
                {
                    logger.LogInformation(
                        "Demo seller {Email} already exists; documented password verified. Password unchanged.",
                        email);
                }
                else
                {
                    logger.LogWarning(
                        "Demo seller {Email} already exists with a DIFFERENT password. " +
                        "Its password was NOT changed, so the documented demo password may not work for it.",
                        email);
                }
            }

            // 2. Ensure the Seller role (no-op if already assigned).
            if (!await userManager.IsInRoleAsync(user, "Seller"))
            {
                await userManager.AddToRoleAsync(user, "Seller");
            }

            // 3. Ensure an Approved SellerProfile - create only if missing.
            var profileExists = await dbContext.SellerProfiles
                .AnyAsync(p => p.UserId == user.Id);

            if (!profileExists)
            {
                dbContext.SellerProfiles.Add(new SellerProfile
                {
                    UserId = user.Id,
                    StoreName = storeName,
                    Description = description,
                    Logo = string.Empty,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync();
                logger.LogInformation("Created approved seller profile for {Email}", email);
            }
        }
    }
}
