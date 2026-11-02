using GlowCare.API.Data;
using GlowCare.API.Helpers;
using GlowCare.API.Middleware;
using GlowCare.API.Models.Identity;
using GlowCare.API.Services.Implementations;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration
// Bind strongly typed settings sections so values are never accessed by magic strings.
// Secrets (JWT key, Paymob credentials, admin password) come from
// User Secrets or environment variables - never from committed files.
// ---------------------------------------------------------------------------
builder.Services.Configure<StoreSettings>(builder.Configuration.GetSection(StoreSettings.SectionName));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<PaymobSettings>(builder.Configuration.GetSection(PaymobSettings.SectionName));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection(AdminSettings.SectionName));

builder.Services.AddControllers();

// ---------------------------------------------------------------------------
// DbContext - EF Core with SQL Server. The DbContext is our Unit of Work
// (no Repository pattern by design).
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------------------
// ASP.NET Core Identity
// - ApplicationUser / IdentityRole keep users and roles in the same database.
// - Password rules are strict but reasonable for a student project.
// - Lockout protects login from simple brute force attempts.
// ---------------------------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ---------------------------------------------------------------------------
// JWT Authentication
// The token is validated with the same secret/issuer/audience that AuthService
// will use when generating tokens (Phase 7).
// ---------------------------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings section is missing from configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1) // default is 5 minutes; keep it tight
    };
});

// ---------------------------------------------------------------------------
// Services - no Repository pattern by design: services use AppDbContext
// directly as the Unit of Work. Controllers depend on interfaces only.
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Catalog (Phase 8a)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ISkinTypeService, SkinTypeService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();

// Cart / Orders / Promo codes (Phase 8b)
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPromoCodeService, PromoCodeService>();

// Paymob card payments (Phase 8c)
// CheckoutPreparer = shared checkout/money engine used by OrderService AND PaymentService.
builder.Services.AddScoped<CheckoutPreparer>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpClient(); // IHttpClientFactory for Paymob API calls

// Reviews / Wishlist / Dashboards / Banners (Phase 8d)
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBannerService, BannerService>();

// ---------------------------------------------------------------------------
// CORS - allow the React frontend origin (localhost and ngrok).
// ---------------------------------------------------------------------------
const string CorsPolicy = "Frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(
            "http://localhost:5173",
            "https://dentist-residue-appliance.ngrok-free.dev",
            "https://outboard-glutinous-clinic.ngrok-free.dev"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

// ---------------------------------------------------------------------------
// Swagger / OpenAPI with JWT Bearer support so endpoints can be tested in Phase 9.
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "GlowCare API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here (without the 'Bearer ' prefix)."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Global exception handling - FIRST in the pipeline. Converts AppException
// (business errors from services) into its HTTP status + JSON body, and
// everything else into a generic 500 without leaking internals.
// ---------------------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ---------------------------------------------------------------------------
// Database initialization (Development environment only).
// In Development we apply pending migrations and seed data automatically so
// every team member's local database stays in sync without extra commands.
// In production, migrations are applied explicitly by the designated developer
// ("dotnet ef database update") - per the team workflow rules.
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await SeedData.InitializeAsync(app.Services);
    // Fixed academic demo seller accounts (Development only - permanent demo
    // credentials, never seeded in production).
    await GlowCare.API.Data.Seeders.DemoAccountSeeder.InitializeAsync(app.Services);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
