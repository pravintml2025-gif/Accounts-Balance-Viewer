namespace Adra.Application.DTOs;

public class BalanceSummaryDto
{
    public string AccountName { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public int RecordCount { get; set; }
    public string FormattedAmount => TotalAmount.ToString("C2");
    public string PeriodDisplay => $"{Year}-{Month:D2}";
}
