using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Infrastructure.Migrations;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Data.Repositories;
using ATMS.Data.Mongo.Modules;
using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ATMS.Admin.Data.Modules;

public static class DataAccessModule
{
    public static IServiceCollection AddAdminData(
        this IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(nameof(DatabaseOptions)).Get<DatabaseOptions>() 
                        ?? throw new ConfigurationException(ConfigurationErrorType.DatabaseSectionNotFound,
                            $"Configuration for section '{nameof(DatabaseOptions)}' is not found or could not be loaded.");
        
        services.AddDbContext<AdminDbContext>(options => options.UseNpgsql(dbOptions.SqlConnection));

        services.AddSingleton<IMongoClient>(_ => new MongoClient(dbOptions.MongoConnection));

        services.AddMongoDbModule(dbOptions.MongoDatabase);

        services.AddScoped<IMigrationRunner, MigrationRunner<AdminDbContext>>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
