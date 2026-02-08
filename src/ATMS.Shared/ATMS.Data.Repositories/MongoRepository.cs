using ATMS.Data.Interfaces;
using ATMS.Data.Mongo.Interfaces;
using ATMS.Data.Repositories.Interfaces;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace ATMS.Data.Repositories;

public class MongoRepository<TEntity> : IMongoRepository<TEntity>
    where TEntity : IEntity
{
    private readonly IMongoCollection<TEntity> _collection;

    public MongoRepository(IMongoContext context)
    {
        _collection = context.GetCollection<TEntity>();
    }

    public Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return _collection
            .Find(filter)
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _collection.ReplaceOneAsync(
            x => x.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken
        );
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _collection.DeleteOneAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return _collection.Find(filter).AnyAsync(cancellationToken);
    }

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return _collection.CountDocumentsAsync(filter, null, cancellationToken);
    }

    public Task<List<TEntity>> GetAsync()
    {
        return _collection
            .Find(_ => true)
            .ToListAsync();
    }
}
