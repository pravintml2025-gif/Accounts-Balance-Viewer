using Adra.Core.Entities;

namespace Adra.Core.Interfaces.Repositories;

public interface IBalanceHistoryRepository
{
    Task UpsertAsync(Guid accountId, int year, int month, decimal amount, Guid userId, CancellationToken ct);
    Task<List<BalanceHistory>> GetAllAsync(CancellationToken ct);
    Task<List<BalanceHistory>> GetLatestAsync(CancellationToken ct);
    Task<List<BalanceHistory>> GetByPeriodAsync(int year, int month, CancellationToken ct);
}
