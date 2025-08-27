namespace Adra.Application.DTOs;

public class BalanceDto
{
    public string Account { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}
