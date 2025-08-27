using Microsoft.AspNetCore.Identity;
using Adra.Core.Entities;

namespace Adra.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;

    public User ToDomainUser() => new()
    {
        Id = Id,
        Username = UserName ?? "",
        Email = Email ?? "",
        IsActive = IsActive,
        CreatedDate = DateTime.UtcNow
    };

    public static ApplicationUser FromDomainUser(User user) => new()
    {
        Id = user.Id,
        UserName = user.Username,
        Email = user.Email,
        IsActive = user.IsActive
    };
}

public class ApplicationRole : IdentityRole<Guid>
{
    public Role ToDomainRole() => new()
    {
        Id = Id,
        Name = Name ?? ""
    };

    public static ApplicationRole FromDomainRole(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name
    };
}
