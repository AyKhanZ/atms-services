using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class ValidationModule
{
    public static IServiceCollection AddValidationServices(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ValidationModule).Assembly, includeInternalTypes: true);

        return services;
    }
}