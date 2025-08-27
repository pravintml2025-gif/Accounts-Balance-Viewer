using Microsoft.EntityFrameworkCore;
using Adra.Core.Entities;
using Adra.Core.Interfaces.Repositories;
using Adra.Infrastructure.Data;

namespace Adra.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByNameAsync(string name, CancellationToken ct)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Name == name, ct);
    }

    public async Task AddAsync(Account account, CancellationToken ct)
    {
        await _context.Accounts.AddAsync(account, ct);
    }

    public async Task<List<Account>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
    }
}
