using ATMS.Data.Interfaces;
using System.Linq.Expressions;

namespace ATMS.Data.Repositories.Interfaces;

public interface IMongoRepository<TEntity>
    where TEntity : IEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // DO LATER !!!!!!!!
    Task<List<TEntity>> GetAsync(Expression<Func<TEntity,bool>> filter, CancellationToken cancellationToken = default);
    // REMUVE LATER !!!!!!!!!!
    Task<List<TEntity>> GetAsync();

    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
}
