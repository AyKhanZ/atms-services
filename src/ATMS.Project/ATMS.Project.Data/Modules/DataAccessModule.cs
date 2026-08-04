using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Services;
using ATMS.Project.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Data.Modules;

public static class DataAccessModule
{
    public static IServiceCollection AddProjectData(
        this IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(nameof(ProjectDatabaseOptions)).Get<ProjectDatabaseOptions>() 
                        ?? throw new ConfigurationException(ConfigurationErrorType.DatabaseSectionNotFound,
                            string.Format(LogMessages.ConfigSectionNotFound, nameof(ProjectDatabaseOptions)));
        
        services.AddDbContext<ProjectDbContext>(options => options.UseNpgsql(dbOptions.SqlConnection));

        services.AddScoped<IDictionariesRepository, DictionariesRepository>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IWorkProjectRepository, WorkProjectRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IEntityCodeGenerator, EntityCodeGenerator>();
        services.AddScoped<IHealthRepository, HealthRepository>();
        services.AddScoped<IInboxRepository, InboxRepository>();
        
        return services;
    }
}
