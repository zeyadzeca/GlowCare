using GlowCare.Import.Models.Catalog;
using GlowCare.Import.Models.Identity;
using GlowCare.Import.Models.User;
using GlowCare.Import.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.Import;

public class ImportDbContext : IdentityDbContext<ApplicationUser>
{
    public ImportDbContext(DbContextOptions<ImportDbContext> options) : base(options)
    {
    }

    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<SkinType> SkinTypes => Set<SkinType>();
    public DbSet<ProductSkinType> ProductSkinTypes => Set<ProductSkinType>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<ProductIngredient> ProductIngredients => Set<ProductIngredient>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Address>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Addresses)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SellerProfile>(entity =>
        {
            entity.HasIndex(s => s.UserId).IsUnique();
            entity.Property(s => s.ApprovalStatus).HasConversion<int>();
            entity.HasOne(s => s.User)
                  .WithOne(u => u.SellerProfile)
                  .HasForeignKey<SellerProfile>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

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

        builder.Entity<Ingredient>(entity =>
        {
            entity.HasIndex(i => i.Name).IsUnique();
        });

        builder.Entity<SkinType>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
        });

        builder.Entity<Product>(entity =>
        {
            entity.Property(p => p.RowVersion).IsRowVersion();
            entity.Property(p => p.IsActive).HasDefaultValue(true);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(p => p.Seller)
                  .WithMany()
                  .HasForeignKey(p => p.SellerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Product_Price_Positive", "[Price] > 0");
                t.HasCheckConstraint("CK_Product_Stock_NonNegative", "[StockQuantity] >= 0");
            });
        });

        builder.Entity<ProductImage>(entity =>
        {
            entity.HasOne(i => i.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

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
}
