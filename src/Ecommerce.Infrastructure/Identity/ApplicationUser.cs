using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7();
    }

    public string DisplayName { get; set; } = string.Empty;
}
