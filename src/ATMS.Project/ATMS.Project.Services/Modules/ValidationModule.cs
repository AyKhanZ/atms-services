using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class ValidationModule
{
    public static IServiceCollection AddValidationServices(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ValidationModule).Assembly, includeInternalTypes: true);
        return services;
    }
}
