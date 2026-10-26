namespace Ecommerce.Contracts.Identity;

public sealed record LoginRequest(string Username, string Password);

public sealed record RegisterRequest(string Username, string Email, string Password);

public sealed record CurrentUserResponse(Guid UserId, string Username, IReadOnlyList<string> Roles);
