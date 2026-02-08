using ATMS.Admin.Data.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class AdminServicesModule
{
    public static IServiceCollection AddAdminServices(
        this IServiceCollection services,string sqlConnection, string mongoConnection)
    {
        services.AddValidationServices();
        services.AddAdminData(sqlConnection, mongoConnection);
        services.AddHandlerServices();
        services.AddMapperServices();

        return services;
    }
}
