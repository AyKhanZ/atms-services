using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Infrastructure.Migrations;
using ATMS.Admin.Data.Repositories;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Data.Mongo.Modules;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
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
        var dbOptions = configuration.GetSection(nameof(AdminDatabaseOptions)).Get<AdminDatabaseOptions>() 
                        ?? throw new ConfigurationException(ConfigurationErrorType.DatabaseSectionNotFound,
                            string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(AdminDatabaseOptions)));
        
        services.AddDbContext<AdminDbContext>(options => options.UseNpgsql(dbOptions.SqlConnection));

        services.AddSingleton<IMongoClient>(_ => new MongoClient(dbOptions.MongoConnection));

        services.AddMongoDbModule(dbOptions.MongoDatabase);

        services.AddScoped<IMigrationRunner, MigrationRunner<AdminDbContext>>();

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        return services;
    }
}
