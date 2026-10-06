using ATMS.Project.Services.Invitations;
using ATMS.Project.Services.Invitations.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class InvitationsModule
{
    public static IServiceCollection AddInvitationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IWorkProjectInvitationService, WorkProjectInvitationService>();

        return services;
    }
}
