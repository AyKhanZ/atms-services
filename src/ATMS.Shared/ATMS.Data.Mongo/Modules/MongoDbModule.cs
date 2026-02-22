using ATMS.Data.Mongo.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ATMS.Data.Mongo.Modules;

public static class MongoDbModule
{
    public static IServiceCollection AddMongoDbModule(
        this IServiceCollection services, Func<IServiceProvider, string> databaseNameFactory)
    {
        services.AddScoped<IMongoContext>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var databaseName = databaseNameFactory(sp);
            return new MongoContext(client, databaseName);
        });

        return services;
    }
}
