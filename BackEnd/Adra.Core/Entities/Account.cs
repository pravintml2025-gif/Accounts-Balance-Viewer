using System.ComponentModel.DataAnnotations;

namespace Adra.Core.Entities;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<BalanceHistory> BalanceHistories { get; set; } = new List<BalanceHistory>();
}
