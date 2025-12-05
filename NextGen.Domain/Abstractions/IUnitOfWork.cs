using NextGen.Domain.Entities;

namespace NextGen.Domain.Abstractions;

public interface IUnitOfWork
{
    IRepository<Tenant> Tenants { get; }
    IRepository<Account> Accounts { get; }
    IRepository<Transaction> Transactions { get; }
    IRepository<AppUser> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
