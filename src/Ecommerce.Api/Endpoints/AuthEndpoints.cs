using Ecommerce.Application.Abstractions;
using Ecommerce.Contracts.Identity;
using Ecommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            UserManager<ApplicationUser> users) =>
        {
            if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length is < 3 or > 40)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["username"] = ["Username must be between 3 and 40 characters."],
                });
            }

            var user = new ApplicationUser
            {
                UserName = request.Username.Trim(),
                Email = request.Email?.Trim(),
                DisplayName = request.Username.Trim(),
                EmailConfirmed = true,
            };

            var created = await users.CreateAsync(user, request.Password);

            if (!created.Succeeded)
            {
                return Results.ValidationProblem(created.Errors
                    .GroupBy(error => error.Code)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
            }

            await users.AddToRoleAsync(user, "Customer");

            return Results.Created($"/api/v1/users/{user.Id}", new { user.Id, user.UserName });
        })
        .AllowAnonymous()
        .WithName("Register");

        group.MapPost("/login", async (
            LoginRequest request,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var result = await signInManager.PasswordSignInAsync(
                request.Username,
                request.Password,
                isPersistent: true,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return Results.NoContent();
            }

            if (result.IsLockedOut)
            {
                return Results.Problem(
                    detail: "Too many failed attempts. Try again in a few minutes.",
                    statusCode: StatusCodes.Status423Locked,
                    title: "Account locked");
            }

            return Results.Problem(
                detail: "Invalid username or password.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized");
        })
        .AllowAnonymous()
        .WithName("Login");

        group.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Logout");

        group.MapGet("/me", (ICurrentUser currentUser) => currentUser.IsAuthenticated
                ? Results.Ok(new CurrentUserResponse(
                    currentUser.UserId!.Value,
                    currentUser.Username!,
                    currentUser.Roles))
                : Results.Unauthorized())
            .AllowAnonymous()
            .WithName("GetCurrentUser");

        return app;
    }
}
