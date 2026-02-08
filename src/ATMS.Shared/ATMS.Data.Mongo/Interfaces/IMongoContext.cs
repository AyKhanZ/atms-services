using MongoDB.Driver;

namespace ATMS.Data.Mongo.Interfaces;

public interface IMongoContext
{
    IMongoCollection<TEntity> GetCollection<TEntity>();
}
