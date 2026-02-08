using ATMS.Data.Mongo.Extensions;
using ATMS.Data.Mongo.Interfaces;
using MongoDB.Driver;

namespace ATMS.Data.Mongo;

public class MongoContext(IMongoClient client, string databaseName) : IMongoContext
{
    private readonly IMongoDatabase _database = client.GetDatabase(databaseName);

    public IMongoCollection<TEntity> GetCollection<TEntity>()
    {
        var attribute = typeof(TEntity)
            .GetCustomAttributes(typeof(MongoCollectionAttribute), false)
            .FirstOrDefault() as MongoCollectionAttribute;

        if (attribute is null)
            throw new InvalidOperationException(
                $"Entity {typeof(TEntity).Name} does not have MongoCollectionAttribute");

        return _database.GetCollection<TEntity>(attribute.Name);
    }
}
