using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Infrastructure.Migrations;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Data.Repositories;
using ATMS.Data.Mongo.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ATMS.Admin.Data.Modules;

public static class DataAccessModule
{
    public static IServiceCollection AddAdminData(
        this IServiceCollection services,string sqlConnection, string mongoConnection)
    {
        services.AddDbContext<AdminDbContext>(options => options.UseNpgsql(sqlConnection));
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));
        services.AddMongoDbModule("atms.admin");

        services.AddScoped<IMigrationRunner, MigrationRunner<AdminDbContext>>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }
}
