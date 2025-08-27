using Adra.Application.DTOs;

namespace Adra.Application.Interfaces;

public interface IGetBalancesService
{
    Task<List<BalanceDto>> GetLatestAsync(CancellationToken ct);
    Task<List<BalanceDto>> GetByPeriodAsync(int year, int month, CancellationToken ct);

    // Summary methods
    Task<List<BalanceSummaryDto>> GetSummaryAsync(CancellationToken ct);
    Task<List<BalanceSummaryDto>> GetSummaryByPeriodAsync(int year, int month, CancellationToken ct);
}
