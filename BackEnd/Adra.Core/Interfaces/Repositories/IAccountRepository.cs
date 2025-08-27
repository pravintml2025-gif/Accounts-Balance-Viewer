using Adra.Core.Entities;

namespace Adra.Core.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByNameAsync(string name, CancellationToken ct);
    Task AddAsync(Account account, CancellationToken ct);
    Task<List<Account>> GetAllAsync(CancellationToken ct);
}
