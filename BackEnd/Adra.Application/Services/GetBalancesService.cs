using Adra.Application.DTOs;
using Adra.Application.Interfaces;
using Adra.Core.Interfaces.Repositories;

namespace Adra.Application.Services;

public class GetBalancesService : IGetBalancesService
{
    private readonly IBalanceHistoryRepository _balanceHistoryRepository;

    public GetBalancesService(IBalanceHistoryRepository balanceHistoryRepository)
    {
        _balanceHistoryRepository = balanceHistoryRepository;
    }

    public async Task<List<BalanceDto>> GetLatestAsync(CancellationToken ct)
    {
        var balances = await _balanceHistoryRepository.GetLatestAsync(ct);

        return balances.Select(b => new BalanceDto
        {
            Account = b.Account.Name,
            Amount = b.Amount,
            Year = b.Year,
            Month = b.Month
        }).ToList();
    }

    public async Task<List<BalanceDto>> GetByPeriodAsync(int year, int month, CancellationToken ct)
    {
        var balances = await _balanceHistoryRepository.GetByPeriodAsync(year, month, ct);

        return balances.Select(b => new BalanceDto
        {
            Account = b.Account.Name,
            Amount = b.Amount,
            Year = b.Year,
            Month = b.Month
        }).ToList();
    }

    public async Task<List<BalanceSummaryDto>> GetSummaryAsync(CancellationToken ct)
    {
        var allBalances = await _balanceHistoryRepository.GetAllAsync(ct);

        var summaries = allBalances
            .GroupBy(b => new { b.Account.Name, b.Account.Id })
            .Select(g => new BalanceSummaryDto
            {
                AccountName = g.Key.Name,
                AccountId = g.Key.Id,
                Year = g.Max(b => b.Year), // Use the most recent year for this account
                Month = g.Where(b => b.Year == g.Max(x => x.Year)).Max(b => b.Month), // Most recent month in the most recent year
                TotalAmount = g.Sum(b => b.Amount), // Sum all amounts for this account across all periods
                LastUpdatedAt = g.Max(b => b.UploadedAt),
                RecordCount = g.Count() // Total number of balance records for this account
            })
            .OrderBy(s => s.AccountName)
            .ToList();

        return summaries;
    }

    public async Task<List<BalanceSummaryDto>> GetSummaryByPeriodAsync(int year, int month, CancellationToken ct)
    {
        var balances = await _balanceHistoryRepository.GetByPeriodAsync(year, month, ct);

        var summaries = balances
            .GroupBy(b => new { b.Account.Name, b.Account.Id, b.Year, b.Month })
            .Select(g => new BalanceSummaryDto
            {
                AccountName = g.Key.Name,
                AccountId = g.Key.Id,
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAmount = g.Sum(b => b.Amount),
                LastUpdatedAt = g.Max(b => b.UploadedAt),
                RecordCount = g.Count()
            })
            .ToList();

        return summaries;
    }
}
