using System.Linq.Expressions;

namespace NextGen.Domain.Abstractions;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
