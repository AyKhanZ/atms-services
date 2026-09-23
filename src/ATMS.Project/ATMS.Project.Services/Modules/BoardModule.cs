using ATMS.Project.Services.Board;
using ATMS.Project.Services.Board.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class BoardModule
{
    public static IServiceCollection AddBoardServices(
        this IServiceCollection services)
    {
        services.AddScoped<IWorkTaskBoardPositionService, WorkTaskBoardPositionService>();

        return services;
    }
}
