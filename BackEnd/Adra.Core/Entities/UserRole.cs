using System.ComponentModel.DataAnnotations;

namespace Adra.Core.Entities;

public class UserRole
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
