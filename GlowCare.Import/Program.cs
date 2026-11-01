using CsvHelper;
using CsvHelper.Configuration;
using GlowCare.Import.Models.Catalog;
using GlowCare.Import.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Error: Connection string not found in appsettings.json");
    return 1;
}

var optionsBuilder = new DbContextOptionsBuilder<ImportDbContext>();
optionsBuilder.UseSqlServer(connectionString);

await using var context = new ImportDbContext(optionsBuilder.Options);

Console.WriteLine("=== GlowCare Product Import Utility ===");
Console.WriteLine($"CSV Path: ../Dataset/1-200.csv");
Console.WriteLine($"Database: {connectionString}");
Console.WriteLine();

// Step 1: Check for approved sellers
Console.WriteLine("Step 1: Retrieving approved sellers...");
var approvedSellers = await context.SellerProfiles
    .Where(s => s.ApprovalStatus == SellerApprovalStatus.Approved)
    .Include(s => s.User)
    .ToListAsync();

if (approvedSellers.Count < 2)
{
    Console.WriteLine($"ERROR: Only {approvedSellers.Count} approved seller(s) found in database.");
    Console.WriteLine("At least 2 approved sellers are required for product distribution.");
    Console.WriteLine("Please create seller accounts through the Admin approval workflow first.");
    return 1;
}

Console.WriteLine($"Found {approvedSellers.Count} approved sellers:");
foreach (var seller in approvedSellers)
{
    Console.WriteLine($"  - {seller.User.FullName} (ID: {seller.UserId})");
}
Console.WriteLine();

// Step 2: Read CSV
Console.WriteLine("Step 2: Reading CSV file...");
var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Dataset", "1-200.csv");
if (!File.Exists(csvPath))
{
    Console.WriteLine($"ERROR: CSV file not found at {csvPath}");
    return 1;
}

using var reader = new StreamReader(csvPath);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
csv.Context.RegisterClassMap<SkincareProductMap>();
var records = csv.GetRecords<SkincareProduct>().ToList();

Console.WriteLine($"Total rows in CSV: {records.Count}");
Console.WriteLine();

// Step 3: Import products
Console.WriteLine("Step 3: Starting product import...");
var importResult = await ImportProductsAsync(context, records, approvedSellers);

// Step 4: Generate report
Console.WriteLine();
Console.WriteLine("=== IMPORT REPORT ===");
Console.WriteLine($"Total CSV rows: {records.Count}");
Console.WriteLine($"Valid products: {importResult.ValidProducts}");
Console.WriteLine($"Imported products: {importResult.ImportedProducts}");
Console.WriteLine($"Skipped products (duplicates): {importResult.SkippedProducts}");
Console.WriteLine($"Failed records: {importResult.FailedRecords}");
Console.WriteLine();
Console.WriteLine("=== PRICING REPORT ===");
Console.WriteLine("Pricing distribution by category:");
foreach (var (category, count) in importResult.PricingByCategory.OrderByDescending(x => x.Value))
{
    Console.WriteLine($"  {category}: {count} products");
}
Console.WriteLine($"Minimum price: {importResult.MinPrice} EGP");
Console.WriteLine($"Maximum price: {importResult.MaxPrice} EGP");
Console.WriteLine($"Products with fallback pricing: {importResult.FallbackPricingCount}");
Console.WriteLine();
Console.WriteLine("=== SELLER DISTRIBUTION ===");
foreach (var (sellerId, stats) in importResult.SellerStats)
{
    var seller = approvedSellers.FirstOrDefault(s => s.UserId == sellerId);
    var sellerName = seller?.User.FullName ?? sellerId;
    Console.WriteLine($"  {sellerName}:");
    Console.WriteLine($"    Brands: {stats.BrandCount}");
    Console.WriteLine($"    Products: {stats.ProductCount}");
}
Console.WriteLine();
Console.WriteLine("=== DATABASE RECORDS ===");
Console.WriteLine($"Brands created: {importResult.BrandsCreated}");
Console.WriteLine($"Brands reused: {importResult.BrandsReused}");
Console.WriteLine($"Categories created: {importResult.CategoriesCreated}");
Console.WriteLine($"Categories reused: {importResult.CategoriesReused}");
Console.WriteLine($"Ingredients created: {importResult.IngredientsCreated}");
Console.WriteLine($"Ingredients reused: {importResult.IngredientsReused}");
Console.WriteLine($"ProductIngredient relationships: {importResult.ProductIngredientCount}");
Console.WriteLine($"ProductImage records: {importResult.ProductImageCount}");
Console.WriteLine();
Console.WriteLine("=== IMAGE DOWNLOADS ===");
Console.WriteLine($"Successfully downloaded: {importResult.ImagesDownloaded}");
Console.WriteLine($"Failed downloads: {importResult.FailedImageDownloads}");
Console.WriteLine($"Invalid URLs: {importResult.InvalidImageUrls}");
Console.WriteLine();

return 0;

async Task<ImportResult> ImportProductsAsync(
    ImportDbContext context,
    List<SkincareProduct> records,
    List<SellerProfile> approvedSellers)
{
    var result = new ImportResult();
    var brandSellerMap = new Dictionary<string, string>(); // brand name -> seller ID
    var sellerIndex = 0;

    // Preload existing data for efficiency
    var existingBrands = await context.Brands.ToDictionaryAsync(b => NormalizeName(b.Name));
    var existingCategories = await context.Categories.ToDictionaryAsync(c => NormalizeName(c.Name));
    var existingIngredients = await context.Ingredients.ToDictionaryAsync(i => NormalizeName(i.Name));
    var existingProductNames = await context.Products.Select(p => NormalizeName(p.Name)).ToListAsync();

    // Create wwwroot directory for images if it doesn't exist
    var imageDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "GlowCare.API", "wwwroot", "products");
    Directory.CreateDirectory(imageDir);

    foreach (var record in records)
    {
        try
        {
            result.ValidProducts++;

            // Check for duplicate product
            var normalizedName = NormalizeName(record.ProductName);
            if (existingProductNames.Contains(normalizedName))
            {
                result.SkippedProducts++;
                continue;
            }

            // Get or create brand
            var brandName = NormalizeName(record.Brand);
            if (!existingBrands.TryGetValue(brandName, out var brand))
            {
                brand = new Brand { Name = record.Brand };
                context.Brands.Add(brand);
                await context.SaveChangesAsync();
                existingBrands[brandName] = brand;
                result.BrandsCreated++;
            }
            else
            {
                result.BrandsReused++;
            }

            // Assign seller to brand (deterministic)
            if (!brandSellerMap.ContainsKey(brandName))
            {
                brandSellerMap[brandName] = approvedSellers[sellerIndex].UserId;
                sellerIndex = (sellerIndex + 1) % approvedSellers.Count;
            }
            var sellerId = brandSellerMap[brandName];

            // Get or create category
            var categoryName = NormalizeName(record.Category);
            if (!existingCategories.TryGetValue(categoryName, out var category))
            {
                category = new Category { Name = record.Category };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                existingCategories[categoryName] = category;
                result.CategoriesCreated++;
            }
            else
            {
                result.CategoriesReused++;
            }

            // Determine price
            var price = DeterminePrice(record, category.Name);
            result.UpdatePricingStats(category.Name, price);

            // Extract volume from product name
            var volume = ExtractVolume(record.ProductName);

            // Extract SPF if present
            var spf = ExtractSPF(record.ProductName);

            // Create product
            var product = new Product
            {
                Name = record.ProductName,
                Description = $"Imported from SkinSafe dataset. Brand: {record.Brand}, Category: {record.Category}.",
                Price = price,
                StockQuantity = 50,
                BrandId = brand.Id,
                CategoryId = category.Id,
                SellerId = sellerId,
                Volume = volume,
                SPF = spf,
                IsActive = true
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();
            result.ImportedProducts++;
            existingProductNames.Add(normalizedName);

            // Update seller stats
            if (!result.SellerStats.ContainsKey(sellerId))
            {
                result.SellerStats[sellerId] = (0, 0);
            }
            var currentStats = result.SellerStats[sellerId];
            result.SellerStats[sellerId] = (currentStats.BrandCount + 1, currentStats.ProductCount + 1);

            // Process ingredients
            if (!string.IsNullOrWhiteSpace(record.Ingredients))
            {
                var ingredients = ParseIngredients(record.Ingredients);
                foreach (var ingName in ingredients)
                {
                    var normalIngName = NormalizeName(ingName);
                    if (!existingIngredients.TryGetValue(normalIngName, out var ingredient))
                    {
                        ingredient = new Ingredient { Name = ingName };
                        context.Ingredients.Add(ingredient);
                        await context.SaveChangesAsync();
                        existingIngredients[normalIngName] = ingredient;
                        result.IngredientsCreated++;
                    }
                    else
                    {
                        result.IngredientsReused++;
                    }

                    var productIngredient = new ProductIngredient
                    {
                        ProductId = product.Id,
                        IngredientId = ingredient.Id
                    };
                    context.ProductIngredients.Add(productIngredient);
                    result.ProductIngredientCount++;
                }
            }

            // Download and save image
            if (!string.IsNullOrWhiteSpace(record.ImageUrl) && Uri.TryCreate(record.ImageUrl, UriKind.Absolute, out var imageUri))
            {
                try
                {
                    var imageFileName = $"{product.Id}{Path.GetExtension(imageUri.AbsolutePath)}";
                    var imagePath = Path.Combine(imageDir, imageFileName);

                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(imageUri);
                    await File.WriteAllBytesAsync(imagePath, imageBytes);

                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = $"/products/{imageFileName}"
                    };
                    context.ProductImages.Add(productImage);
                    result.ImagesDownloaded++;
                    result.ProductImageCount++;
                }
                catch
                {
                    result.FailedImageDownloads++;
                }
            }
            else
            {
                result.InvalidImageUrls++;
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing product '{record.ProductName}': {ex.Message}");
            result.FailedRecords++;
        }
    }

    return result;
}

decimal DeterminePrice(SkincareProduct record, string category)
{
    var categoryLower = category.ToLower();
    var nameLower = record.ProductName.ToLower();

    // Price ranges
    decimal minPrice, maxPrice;

    if (categoryLower.Contains("cleanser") || nameLower.Contains("cleanser"))
    {
        minPrice = 250; maxPrice = 450;
    }
    else if (categoryLower.Contains("toner") || nameLower.Contains("toner"))
    {
        minPrice = 200; maxPrice = 400;
    }
    else if (categoryLower.Contains("serum") || nameLower.Contains("serum"))
    {
        minPrice = 350; maxPrice = 700;
    }
    else if (categoryLower.Contains("moisturizer") || nameLower.Contains("moisturizer") || categoryLower.Contains("cream"))
    {
        minPrice = 300; maxPrice = 650;
    }
    else if (categoryLower.Contains("sunscreen") || nameLower.Contains("spf") || nameLower.Contains("sun"))
    {
        minPrice = 350; maxPrice = 700;
    }
    else if (categoryLower.Contains("eye") || nameLower.Contains("eye"))
    {
        minPrice = 400; maxPrice = 800;
    }
    else if (categoryLower.Contains("mask") || nameLower.Contains("mask"))
    {
        minPrice = 200; maxPrice = 450;
    }
    else if (categoryLower.Contains("body") || nameLower.Contains("body"))
    {
        minPrice = 250; maxPrice = 600;
    }
    else
    {
        minPrice = 300; maxPrice = 500;
    }

    // Deterministic pricing based on product name hash
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(record.ProductName));
    var hashInt = BitConverter.ToUInt32(hash, 0);
    var normalized = (double)hashInt / uint.MaxValue;
    var price = minPrice + (decimal)(normalized * (maxPrice - minPrice));

    return Math.Round(price, 2);
}

string ExtractVolume(string productName)
{
    // Look for patterns like "30 mL", "50g", "1 fl oz", etc.
    var match = System.Text.RegularExpressions.Regex.Match(
        productName,
        @"(\d+(?:\.\d+)?)\s*(?:fl\s*oz|ml|g|oz)",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    return match.Success ? match.Value : "Standard";
}

int? ExtractSPF(string productName)
{
    var match = System.Text.RegularExpressions.Regex.Match(
        productName,
        @"SPF\s*(\d+)",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    return match.Success && int.TryParse(match.Groups[1].Value, out var spf) ? spf : null;
}

List<string> ParseIngredients(string ingredients)
{
    // Split by comma and clean up
    return ingredients.Split(',')
        .Select(i => i.Trim())
        .Where(i => !string.IsNullOrWhiteSpace(i))
        .ToList();
}

string NormalizeName(string name)
{
    return name.Trim().ToLowerInvariant();
}

// CSV Mapping
public class SkincareProduct
{
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string UsageType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Ingredients { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ProductUrl { get; set; } = string.Empty;
}

public class SkincareProductMap : ClassMap<SkincareProduct>
{
    public SkincareProductMap()
    {
        Map(m => m.ProductName).Name("product_name");
        Map(m => m.Brand).Name("brand");
        Map(m => m.UsageType).Name("usage_type");
        Map(m => m.Category).Name("category");
        Map(m => m.Ingredients).Name("ingredients");
        Map(m => m.ImageUrl).Name("image_url");
        Map(m => m.ProductUrl).Name("product_url");
    }
}

public class ImportResult
{
    public int ValidProducts { get; set; }
    public int ImportedProducts { get; set; }
    public int SkippedProducts { get; set; }
    public int FailedRecords { get; set; }

    public Dictionary<string, int> PricingByCategory { get; } = new();
    public decimal MinPrice { get; set; } = decimal.MaxValue;
    public decimal MaxPrice { get; set; } = decimal.MinValue;
    public int FallbackPricingCount { get; set; }

    public Dictionary<string, (int BrandCount, int ProductCount)> SellerStats { get; } = new();

    public int BrandsCreated { get; set; }
    public int BrandsReused { get; set; }
    public int CategoriesCreated { get; set; }
    public int CategoriesReused { get; set; }
    public int IngredientsCreated { get; set; }
    public int IngredientsReused { get; set; }
    public int ProductIngredientCount { get; set; }
    public int ProductImageCount { get; set; }

    public int ImagesDownloaded { get; set; }
    public int FailedImageDownloads { get; set; }
    public int InvalidImageUrls { get; set; }

    public void UpdatePricingStats(string category, decimal price)
    {
        if (!PricingByCategory.ContainsKey(category))
        {
            PricingByCategory[category] = 0;
        }
        PricingByCategory[category]++;

        if (price < MinPrice) MinPrice = price;
        if (price > MaxPrice) MaxPrice = price;
    }
}
