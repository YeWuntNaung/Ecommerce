using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Primitives;
using Ecommerce.Domain.Vendors;
using Ecommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Persistence;

public static class DatabaseInitialization
{
    /// <summary>Applies migrations and seeds demo data. Safe to run repeatedly.</summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitialization");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(cancellationToken);
        await DbSeeder.SeedAsync(scope.ServiceProvider, configuration, cancellationToken);

        logger.LogInformation("Database is migrated and seeded.");
    }
}

public static class DbSeeder
{
    public const string AdminUsername = "admin";
    public const string VendorUsername = "vendor";
    public const string DemoStoreSlug = "demo-store";

    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in new[] { "Admin", "VendorOwner", "Customer" })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // Demo data is local-first-run only. On a real deployment set
        // Seed__DemoData=false and supply Seed__AdminPassword to bootstrap a
        // single admin account; no demo storefront or products are created.
        if (!configuration.GetValue("Seed:DemoData", true))
        {
            var bootstrapPassword = configuration["Seed:AdminPassword"];

            if (!string.IsNullOrWhiteSpace(bootstrapPassword))
            {
                await EnsureUserAsync(
                    users,
                    AdminUsername,
                    "admin@example.com",
                    "Marketplace Admin",
                    bootstrapPassword,
                    "Admin");
            }

            return;
        }

        if (await db.Vendors.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = await EnsureUserAsync(
            users,
            AdminUsername,
            "admin@example.com",
            "Marketplace Admin",
            configuration["Seed:AdminPassword"] ?? "Admin123!",
            "Admin");

        var vendorOwner = await EnsureUserAsync(
            users,
            VendorUsername,
            "vendor@example.com",
            "Demo Store Owner",
            configuration["Seed:VendorPassword"] ?? "Vendor123!",
            "VendorOwner");

        var now = DateTimeOffset.UtcNow;

        var storeSlug = VendorSlug.Create(DemoStoreSlug).Value;
        var vendor = Vendor.Create(
            "Demo Store",
            storeSlug,
            "A demonstration storefront seeded for development.",
            vendorOwner.Id,
            now);
        vendor.Approve(now);
        db.Vendors.Add(vendor);

        var electronics = Category.Create("Electronics", "electronics", 1);
        var fashion = Category.Create("Fashion", "fashion", 2);
        var home = Category.Create("Home & Living", "home-living", 3);
        db.Categories.AddRange(electronics, fashion, home);

        var earbuds = Product.CreateDraft(
            vendor.Id,
            electronics.Id,
            "Wireless Earbuds",
            "wireless-earbuds",
            "Compact Bluetooth earbuds with a charging case.",
            now);
        earbuds.AddVariant("Black", "EAR-BLK", Money.Kyat(85_000), Weight.FromGrams(200), 25, now);
        earbuds.AddVariant("White", "EAR-WHT", Money.Kyat(85_000), Weight.FromGrams(200), 18, now);
        earbuds.SubmitForReview(now);
        earbuds.Approve(admin.Id, now);

        var watch = Product.CreateDraft(
            vendor.Id,
            electronics.Id,
            "Smart Watch",
            "smart-watch",
            "Fitness tracking watch with a seven day battery.",
            now);
        watch.AddVariant("Graphite", "WATCH-GRA", Money.Kyat(145_000), Weight.FromGrams(350), 12, now);
        watch.SubmitForReview(now);

        db.Products.AddRange(earbuds, watch);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> users,
        string username,
        string email,
        string displayName,
        string password,
        string role)
    {
        var existing = await users.FindByNameAsync(username);

        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true,
        };

        var created = await users.CreateAsync(user, password);

        if (!created.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not seed user '{username}': {string.Join("; ", created.Errors.Select(error => error.Description))}");
        }

        await users.AddToRoleAsync(user, role);
        return user;
    }
}
