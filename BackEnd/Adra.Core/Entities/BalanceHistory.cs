using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adra.Core.Entities;

public class BalanceHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AccountId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UploadedBy { get; set; }

    public virtual Account Account { get; set; } = null!;
    // Note: No navigation to User to avoid circular dependencies with Identity
}
