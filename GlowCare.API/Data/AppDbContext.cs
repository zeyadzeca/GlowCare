using GlowCare.API.Models.Catalog;
using GlowCare.API.Models.Content;
using GlowCare.API.Models.Identity;
using GlowCare.API.Models.Orders;
using GlowCare.API.Models.Promotions;
using GlowCare.API.Models.Reviews;
using GlowCare.API.Models.Shopping;
using GlowCare.API.Models.User;
using GlowCare.API.Models.Wishlist;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Data;

/// <summary>
/// The application database context. It also serves as the Unit of Work
/// (no Repository pattern by design - services use this directly).
///
/// NOTE: This context is built up incrementally across the project phases.
/// Phase 2: Identity + User models. Phase 3: Catalog models.
/// Phase 4: Shopping + Orders models. Phase 5: Promotions, Reviews,
/// Wishlist and Content models. Phase 6: final schema review and verification.
/// Only ONE designated developer edits this file for migrations (team rule).
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Phase 2: User-related tables
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();

    // Phase 3: Catalog tables
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<SkinType> SkinTypes => Set<SkinType>();
    public DbSet<ProductSkinType> ProductSkinTypes => Set<ProductSkinType>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<ProductIngredient> ProductIngredients => Set<ProductIngredient>();

    // Phase 4: Shopping and Orders tables
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Phase 5: Promotions, Reviews, Wishlist and Content tables
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<PromoCodeUsage> PromoCodeUsages => Set<PromoCodeUsage>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Banner> Banners => Set<Banner>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // creates the AspNet* Identity tables

        ConfigureUserModels(builder);
        ConfigureCatalogModels(builder);
        ConfigureShoppingAndOrderModels(builder);
        ConfigurePromotionReviewWishlistModels(builder);

        // ------------------------------------------------------------------
        // Global rule: every decimal column in the database uses (18,2).
        // All decimals in this schema are money/amount values, so one loop
        // keeps precision consistent now and in later phases.
        // ------------------------------------------------------------------
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }
        }
    }

    private static void ConfigureUserModels(ModelBuilder builder)
    {
        // ------------------------------------------------------------------
        // Address: one user can have many addresses.
        // Deleting a user removes their addresses.
        // ------------------------------------------------------------------
        builder.Entity<Address>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Addresses)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------------
        // SellerProfile: one-to-one (optional) with ApplicationUser.
        // UserId is unique -> at most one profile per user.
        // ------------------------------------------------------------------
        builder.Entity<SellerProfile>(entity =>
        {
            entity.HasIndex(s => s.UserId).IsUnique();

            entity.Property(s => s.ApprovalStatus)
                  .HasConversion<int>();

            entity.Property(s => s.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(s => s.User)
                  .WithOne(u => u.SellerProfile)
                  .HasForeignKey<SellerProfile>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCatalogModels(ModelBuilder builder)
    {
        // ------------------------------------------------------------------
        // Category / Brand: unique names, Restrict deletion while products exist.
        // ------------------------------------------------------------------
        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();

            entity.HasMany(c => c.Products)
                  .WithOne(p => p.Category)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Brand>(entity =>
        {
            entity.HasIndex(b => b.Name).IsUnique();

            entity.HasMany(b => b.Products)
                  .WithOne(p => p.Brand)
                  .HasForeignKey(p => p.BrandId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ------------------------------------------------------------------
        // Ingredient / SkinType: unique names.
        // ------------------------------------------------------------------
        builder.Entity<Ingredient>(entity =>
        {
            entity.HasIndex(i => i.Name).IsUnique();
        });

        builder.Entity<SkinType>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
        });

        // ------------------------------------------------------------------
        // Product
        // - Seller: Restrict - a seller with products cannot be deleted.
        // - RowVersion: optimistic concurrency (approved addition) so two
        //   concurrent checkouts can never oversell the same stock.
        // - Check constraints: positive price, non-negative stock.
        // - Indexes: cover the main filtering columns used by the catalog API.
        // ------------------------------------------------------------------
        builder.Entity<Product>(entity =>
        {
            entity.Property(p => p.RowVersion)
                  .IsRowVersion();

            entity.Property(p => p.IsActive)
                  .HasDefaultValue(true);

            entity.Property(p => p.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(p => p.Seller)
                  .WithMany()
                  .HasForeignKey(p => p.SellerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Product_Price_Positive", "[Price] > 0");
                t.HasCheckConstraint("CK_Product_Stock_NonNegative", "[StockQuantity] >= 0");
            });

            entity.HasIndex(p => new { p.CategoryId, p.BrandId, p.IsActive });
            entity.HasIndex(p => p.Price);
            entity.HasIndex(p => p.SellerId);
        });

        // ------------------------------------------------------------------
        // ProductImage: many per product; deleting a product removes its images.
        // ------------------------------------------------------------------
        builder.Entity<ProductImage>(entity =>
        {
            entity.HasOne(i => i.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------------
        // Product <-> SkinType (many-to-many through ProductSkinTypes).
        // Composite primary key prevents duplicate product/skin-type links.
        // ------------------------------------------------------------------
        builder.Entity<ProductSkinType>(entity =>
        {
            entity.HasKey(ps => new { ps.ProductId, ps.SkinTypeId });

            entity.HasOne(ps => ps.Product)
                  .WithMany(p => p.ProductSkinTypes)
                  .HasForeignKey(ps => ps.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ps => ps.SkinType)
                  .WithMany(s => s.ProductSkinTypes)
                  .HasForeignKey(ps => ps.SkinTypeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------------
        // Product <-> Ingredient (many-to-many through ProductIngredients).
        // Composite primary key prevents duplicate product/ingredient links.
        // ------------------------------------------------------------------
        builder.Entity<ProductIngredient>(entity =>
        {
            entity.HasKey(pi => new { pi.ProductId, pi.IngredientId });

            entity.HasOne(pi => pi.Product)
                  .WithMany(p => p.ProductIngredients)
                  .HasForeignKey(pi => pi.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pi => pi.Ingredient)
                  .WithMany(i => i.ProductIngredients)
                  .HasForeignKey(pi => pi.IngredientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureShoppingAndOrderModels(ModelBuilder builder)
    {
        // ------------------------------------------------------------------
        // Cart: exactly one persistent cart per registered user.
        // The unique index on UserId enforces the one-to-one relationship.
        // Deleting a user removes their cart and items.
        // ------------------------------------------------------------------
        builder.Entity<Cart>(entity =>
        {
            entity.HasIndex(c => c.UserId).IsUnique();

            entity.Property(c => c.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Items)
                  .WithOne(i => i.Cart)
                  .HasForeignKey(i => i.CartId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------------
        // CartItem: deleting a product removes it from every cart.
        // ------------------------------------------------------------------
        builder.Entity<CartItem>(entity =>
        {
            entity.HasOne(i => i.Product)
                  .WithMany()
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(t =>
                t.HasCheckConstraint("CK_CartItem_Quantity_Positive", "[Quantity] > 0"));
        });

        // ------------------------------------------------------------------
        // Order
        // - Restrict on the user FK: historical orders are never lost with the account.
        // - PromoCodeId -> PromoCode: Restrict, so deleting a used promo code is
        //   blocked; admins deactivate codes (IsActive = false) instead.
        // - Enum columns stored as int.
        // ------------------------------------------------------------------
        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Status).HasConversion<int>();
            entity.Property(o => o.PaymentMethod).HasConversion<int>();
            entity.Property(o => o.PaymentStatus).HasConversion<int>();

            entity.Property(o => o.OrderDate)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.PromoCode)
                  .WithMany()
                  .HasForeignKey(o => o.PromoCodeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.PromoCodeId);

            entity.HasMany(o => o.OrderItems)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.Payments)
                  .WithOne(p => p.Order)
                  .HasForeignKey(p => p.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.StatusHistory)
                  .WithOne(h => h.Order)
                  .HasForeignKey(h => h.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(t =>
                t.HasCheckConstraint("CK_Order_TotalAmount_NonNegative", "[TotalAmount] >= 0"));
        });

        // ------------------------------------------------------------------
        // OrderItem
        // - ProductName/UnitPrice are purchase-time snapshots.
        // - Restrict on the product FK: order history survives product removal.
        // - Restrict on the seller FK: a seller's historical items are never lost.
        // - SellerId index powers the seller dashboard queries.
        // ------------------------------------------------------------------
        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.FulfillmentStatus).HasConversion<int>();

            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(oi => oi.SellerId);
            entity.HasIndex(oi => oi.OrderId);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_OrderItem_Quantity_Positive", "[Quantity] > 0");
                t.HasCheckConstraint("CK_OrderItem_UnitPrice_NonNegative", "[UnitPrice] >= 0");
            });
        });

        // ------------------------------------------------------------------
        // OrderStatusHistory: enum as int.
        // ------------------------------------------------------------------
        builder.Entity<OrderStatusHistory>(entity =>
        {
            entity.Property(h => h.Status).HasConversion<int>();

            entity.Property(h => h.ChangedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
        });

        // ------------------------------------------------------------------
        // Payment: enum as int. No card details are ever stored here.
        // ------------------------------------------------------------------
        builder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.PaymentMethod).HasConversion<int>();
            entity.Property(p => p.PaymentStatus).HasConversion<int>();

            entity.HasOne(p => p.Order)
                  .WithMany(o => o.Payments)
                  .HasForeignKey(p => p.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.OrderId);
        });
    }

    private static void ConfigurePromotionReviewWishlistModels(ModelBuilder builder)
    {
        // ------------------------------------------------------------------
        // PromoCode
        // - Unique code (case-insensitive by storing uppercase in the service).
        // - Check constraints keep discount values and limits sane.
        // ------------------------------------------------------------------
        builder.Entity<PromoCode>(entity =>
        {
            entity.HasIndex(pc => pc.Code).IsUnique();

            entity.Property(pc => pc.DiscountType).HasConversion<int>();

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PromoCode_DiscountValue_Positive", "[DiscountValue] > 0");
                t.HasCheckConstraint("CK_PromoCode_MinimumOrderAmount_NonNegative", "[MinimumOrderAmount] >= 0");
                t.HasCheckConstraint("CK_PromoCode_UsageLimit_Positive", "[UsageLimit] > 0");
            });
        });

        // ------------------------------------------------------------------
        // PromoCodeUsage
        // - Unique (PromoCodeId, UserId): one use per customer per code,
        //   enforced by the database (approved rule - Decision 12).
        // - Restrict on user/order FKs: usage history is never lost.
        // ------------------------------------------------------------------
        builder.Entity<PromoCodeUsage>(entity =>
        {
            entity.HasIndex(u => new { u.PromoCodeId, u.UserId }).IsUnique();

            entity.Property(u => u.UsedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(u => u.PromoCode)
                  .WithMany(pc => pc.Usages)
                  .HasForeignKey(u => u.PromoCodeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.User)
                  .WithMany()
                  .HasForeignKey(u => u.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Order)
                  .WithMany()
                  .HasForeignKey(u => u.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ------------------------------------------------------------------
        // Review
        // - Unique (UserId, ProductId): one review per customer per product.
        // - Check constraint: rating must be 1..5.
        // ------------------------------------------------------------------
        builder.Entity<Review>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();

            entity.Property(r => r.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Product)
                  .WithMany()
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(t =>
                t.HasCheckConstraint("CK_Review_Rating_Between1And5", "[Rating] BETWEEN 1 AND 5"));
        });

        // ------------------------------------------------------------------
        // Wishlist: one per user (unique UserId). Items cascade with the list.
        // ------------------------------------------------------------------
        builder.Entity<Wishlist>(entity =>
        {
            entity.HasIndex(w => w.UserId).IsUnique();

            entity.HasOne(w => w.User)
                  .WithMany()
                  .HasForeignKey(w => w.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(w => w.Items)
                  .WithOne(i => i.Wishlist)
                  .HasForeignKey(i => i.WishlistId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------------
        // WishlistItem: unique (WishlistId, ProductId) - no duplicate products.
        // Deleting a product removes it from every wishlist.
        // ------------------------------------------------------------------
        builder.Entity<WishlistItem>(entity =>
        {
            entity.HasIndex(i => new { i.WishlistId, i.ProductId }).IsUnique();

            entity.HasOne(i => i.Product)
                  .WithMany()
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------------
        // Banner: simple content table for the home page promotions.
        // ------------------------------------------------------------------
        builder.Entity<Banner>(entity =>
        {
            entity.Property(b => b.IsActive).HasDefaultValue(true);
        });
    }
}
