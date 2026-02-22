using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Infrastructure.Migrations;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Data.Repositories;
using ATMS.Data.Mongo.Modules;
using ATMS.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ATMS.Admin.Data.Modules;

public static class DataAccessModule
{
    public static IServiceCollection AddAdminData(
        this IServiceCollection services)
    {
        services.AddDbContext<AdminDbContext>((provider, options) =>
        {
            var dbOptions = provider.GetRequiredService<AdminDatabaseOptions>();
            options.UseNpgsql(dbOptions.SqlConnection);
        });

        services.AddSingleton<IMongoClient>(provider =>
        {
            var dbOptions = provider.GetRequiredService<AdminDatabaseOptions>();
            return new MongoClient(dbOptions.MongoConnection);
        });

        services.AddMongoDbModule(sp =>
        {
            var dbOptions = sp.GetRequiredService<AdminDatabaseOptions>();
            return dbOptions.MongoDatabase;
        });

        services.AddScoped<IMigrationRunner, MigrationRunner<AdminDbContext>>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
