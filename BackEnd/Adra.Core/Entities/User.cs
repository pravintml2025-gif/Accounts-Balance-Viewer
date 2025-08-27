using System.ComponentModel.DataAnnotations;

namespace Adra.Core.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    // Note: No UploadedBalances navigation to avoid circular dependencies
}
