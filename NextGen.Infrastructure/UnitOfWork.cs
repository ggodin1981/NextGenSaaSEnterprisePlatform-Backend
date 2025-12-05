using NextGen.Domain.Abstractions;
using NextGen.Domain.Entities;
using NextGen.Infrastructure.Data;
using NextGen.Infrastructure.Repositories;

namespace NextGen.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IRepository<Tenant> Tenants { get; }
    public IRepository<Account> Accounts { get; }
    public IRepository<Transaction> Transactions { get; }
    public IRepository<AppUser> Users { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Tenants = new Repository<Tenant>(_context);
        Accounts = new Repository<Account>(_context);
        Transactions = new Repository<Transaction>(_context);
        Users = new Repository<AppUser>(_context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
