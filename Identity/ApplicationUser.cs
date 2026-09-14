using Microsoft.AspNetCore.Identity;

namespace Backend.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public UserStatus Status { get; set; } = UserStatus.Active;
}
