using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NextGen.Domain.Abstractions;

namespace NextGen.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _set;

    public Repository(DbContext context)
    {
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _set.FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = _set.AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _set.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _set.Update(entity);
    }

    public void Delete(T entity)
    {
        _set.Remove(entity);
    }
}
