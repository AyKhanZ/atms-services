//using ATMS.Infrastructure.Options;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace ATMS.Infrastructure;

//public static class ServiceCollectionExtensions
//{
//    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
//    {
//        // DB
//        var dbOptions = configuration.GetSection("Admin:Databases").Get<DatabaseOptions>()
//            ?? throw new Exception("Failed to bind DatabaseOptions from configuration.");

//        services.AddSingleton(dbOptions);

//        return services;
//    }
//}
