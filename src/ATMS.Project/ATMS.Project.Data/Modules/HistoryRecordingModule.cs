using ATMS.Project.Data.Interceptors;
using ATMS.Project.Data.Services;
using ATMS.Project.Data.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Data.Modules;

public static class HistoryRecordingModule
{
    public static IServiceCollection AddHistoryRecording(this IServiceCollection services)
    {
        services.AddSingleton<IHistoryFieldMap, HistoryFieldMap>();
        services.AddScoped<ProjectHistoryInterceptor>();

        return services;
    }
}
