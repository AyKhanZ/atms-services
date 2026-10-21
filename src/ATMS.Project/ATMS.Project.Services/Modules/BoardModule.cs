using ATMS.Project.Services.Domain.Board;
using ATMS.Project.Services.Domain.Board.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class BoardModule
{
    public static IServiceCollection AddBoardServices(
        this IServiceCollection services)
    {
        services.AddScoped<IWorkTaskBoardPositionService, WorkTaskBoardPositionService>();
        services.AddScoped<IWorkTaskBoardPlacementService, WorkTaskBoardPlacementService>();
        services.AddScoped<IBoardColumnCacheService, BoardColumnCacheService>();
        services.AddScoped<IWorkTaskColumnService, WorkTaskColumnService>();

        return services;
    }
}
