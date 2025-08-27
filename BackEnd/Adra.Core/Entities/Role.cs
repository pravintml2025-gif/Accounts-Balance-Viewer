using System.ComponentModel.DataAnnotations;

namespace Adra.Core.Entities;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}
