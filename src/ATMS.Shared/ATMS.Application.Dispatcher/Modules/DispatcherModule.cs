using ATMS.Application.Dispatcher.Behaviors;
using ATMS.Application.Dispatcher.Validation;
using ATMS.Contracts.Requests;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Application.Dispatcher.Modules;

public static class DispatcherModule
{
    public static IServiceCollection AddDispatcherServices(
        this IServiceCollection services)
    {
        services.AddLocalizationBehavior();
        services.AddAccessBehavior();
        services.AddSharedValidationServices();
        services.AddValidationBehavior();

        return services;
    }

    public static IServiceCollection AddAccessBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AccessBehavior<,>));

        return services;
    }

    public static IServiceCollection AddSharedValidationServices(this IServiceCollection services)
    {
        services.AddTransient<IValidator<GetPaginationRequest>, PagedRequestValidator>();
        services.AddTransient<IValidator<GetKeysetPaginationRequest>, KeysetPagedRequestValidator>();

        return services;
    }

    public static IServiceCollection AddRequestProcessingBehaviors(this IServiceCollection services)
    {
        services.AddLocalizationBehavior();
        services.AddValidationBehavior();

        return services;
    }

    public static IServiceCollection AddLocalizationBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LocalizationBehavior<,>));

        return services;
    }

    public static IServiceCollection AddValidationBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
