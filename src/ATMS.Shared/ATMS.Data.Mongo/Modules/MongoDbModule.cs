using ATMS.Data.Mongo.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ATMS.Data.Mongo.Modules;

public static class MongoDbModule
{
    public static IServiceCollection AddMongoDbModule(
        this IServiceCollection services,string database)
    {
        services.AddScoped<IMongoContext>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoContext(client, database);
        });

        return services;
    }
}
