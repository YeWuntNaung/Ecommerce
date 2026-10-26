using Ecommerce.Contracts.Catalog;
using Ecommerce.Contracts.Common;
using Ecommerce.Contracts.Identity;
using Ecommerce.Contracts.Vendors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Testcontainers.PostgreSql;

namespace Ecommerce.Api.IntegrationTests;

public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Database:AutoMigrate", "true");
        builder.UseSetting("Seed:AdminPassword", "Admin123!");
        builder.UseSetting("Seed:VendorPassword", "Vendor123!");
    }
}

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("ecommerce_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public ApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        Factory = new ApiFactory(_postgres.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

public sealed class StorefrontApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private HttpClient CreateClient() => fixture.Factory.CreateClient(
        new WebApplicationFactoryClientOptions { HandleCookies = true });

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Storefront_lists_only_approved_products()
    {
        var client = CreateClient();

        var store = await client.GetFromJsonAsync<StorefrontResponse>("/api/v1/stores/demo-store");
        store.ShouldNotBeNull();
        store.Name.ShouldBe("Demo Store");

        var products = await client.GetFromJsonAsync<PagedResponse<ProductSummaryResponse>>(
            "/api/v1/stores/demo-store/products");

        products.ShouldNotBeNull();
        products.TotalCount.ShouldBe(1);
        products.Items.ShouldContain(product => product.Slug == "wireless-earbuds");
        products.Items.ShouldNotContain(product => product.Slug == "smart-watch");
    }

    [Fact]
    public async Task Unknown_storefront_returns_404_problem_details()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/stores/does-not-exist");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task Admin_can_login_and_see_the_moderation_queue()
    {
        var client = CreateClient();

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("admin", "Admin123!"));
        login.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var pending = await client.GetFromJsonAsync<PagedResponse<PendingProductResponse>>(
            "/api/v1/admin/products/pending");

        pending.ShouldNotBeNull();
        pending.Items.ShouldContain(product => product.Name == "Smart Watch");
    }

    [Fact]
    public async Task Vendor_cannot_read_the_admin_queue()
    {
        var client = CreateClient();

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("vendor", "Vendor123!"));
        login.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var response = await client.GetAsync("/api/v1/admin/products/pending");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_vendor_calls_are_rejected()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/vendor/products");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Outbox_contains_the_seeded_product_events()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.AppDbContext>();

        db.OutboxMessages.ShouldNotBeEmpty();
        db.OutboxMessages.ShouldContain(message => message.Type.Contains("ProductApproved"));
    }
}
