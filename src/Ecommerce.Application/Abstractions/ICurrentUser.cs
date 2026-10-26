namespace Ecommerce.Application.Abstractions;

/// <summary>Identity of the caller, resolved by the API host from the auth cookie.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Username { get; }

    bool IsAuthenticated { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);
}
