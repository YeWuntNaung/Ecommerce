using System.Net;
using Ecommerce.Api.Auth;
using Ecommerce.Api.Endpoints;
using Ecommerce.Api.Middleware;
using Ecommerce.Application;
using Ecommerce.Application.Abstractions;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, loggerConfiguration) => loggerConfiguration
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Composition root: the only place that knows both Application and Infrastructure.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options => options.AddPolicy("spa", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));
}

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("VendorOwner", policy => policy.RequireRole("VendorOwner"));

if (builder.Configuration.GetValue("ReverseProxy:Enabled", false))
{
    // Behind Caddy on the same VM. Trusting only loopback keeps spoofed
    // X-Forwarded-Proto headers from outside from affecting cookie security.
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownProxies.Add(IPAddress.Loopback);
        options.KnownProxies.Add(IPAddress.IPv6Loopback);
    });
}

var app = builder.Build();

if (app.Configuration.GetValue("ReverseProxy:Enabled", false))
{
    app.UseForwardedHeaders();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

if (allowedOrigins.Length > 0)
{
    app.UseCors("spa");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapHealthChecks("/health").AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}

app.MapAuthEndpoints();
app.MapStorefrontEndpoints();
app.MapVendorProductEndpoints();
app.MapAdminProductEndpoints();

if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    await app.Services.InitializeDatabaseAsync(app.Configuration);
}

app.Run();

/// <summary>Exposed so the integration test host can reference the real composition root.</summary>
public partial class Program;
