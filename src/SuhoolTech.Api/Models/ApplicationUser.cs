using Microsoft.AspNetCore.Identity;

namespace SuhoolTech.Api.Models;

public sealed class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
