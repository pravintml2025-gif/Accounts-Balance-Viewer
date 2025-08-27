using Microsoft.EntityFrameworkCore;
using Adra.Core.Entities;
using Adra.Core.Interfaces.Repositories;
using Adra.Infrastructure.Data;

namespace Adra.Infrastructure.Repositories;

public class BalanceHistoryRepository : IBalanceHistoryRepository
{
    private readonly AppDbContext _context;

    public BalanceHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(Guid accountId, int year, int month, decimal amount, Guid userId, CancellationToken ct)
    {
        var existing = await _context.BalanceHistories
            .FirstOrDefaultAsync(b => b.AccountId == accountId && b.Year == year && b.Month == month, ct);

        if (existing != null)
        {
            existing.Amount = amount;
            existing.UploadedAt = DateTime.UtcNow;
            existing.UploadedBy = userId;
        }
        else
        {
            var newBalance = new BalanceHistory
            {
                AccountId = accountId,
                Year = year,
                Month = month,
                Amount = amount,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow
            };
            await _context.BalanceHistories.AddAsync(newBalance, ct);
        }
    }

    public async Task<List<BalanceHistory>> GetAllAsync(CancellationToken ct)
    {
        return await _context.BalanceHistories
            .AsNoTracking()
            .Include(b => b.Account)
            .OrderByDescending(b => b.Year)
            .ThenByDescending(b => b.Month)
            .ThenBy(b => b.Account.Name)
            .ToListAsync(ct);
    }

    public async Task<List<BalanceHistory>> GetLatestAsync(CancellationToken ct)
    {
        var latestPeriod = await _context.BalanceHistories
            .AsNoTracking()
            .OrderByDescending(b => b.Year)
            .ThenByDescending(b => b.Month)
            .Select(b => new { b.Year, b.Month })
            .FirstOrDefaultAsync(ct);

        if (latestPeriod == null)
            return new List<BalanceHistory>();

        return await _context.BalanceHistories
            .AsNoTracking()
            .Include(b => b.Account)
            .Where(b => b.Year == latestPeriod.Year && b.Month == latestPeriod.Month)
            .OrderBy(b => b.Account.Name)
            .ToListAsync(ct);
    }

    public async Task<List<BalanceHistory>> GetByPeriodAsync(int year, int month, CancellationToken ct)
    {
        return await _context.BalanceHistories
            .AsNoTracking()
            .Include(b => b.Account)
            .Where(b => b.Year == year && b.Month == month)
            .OrderBy(b => b.Account.Name)
            .ToListAsync(ct);
    }
}
