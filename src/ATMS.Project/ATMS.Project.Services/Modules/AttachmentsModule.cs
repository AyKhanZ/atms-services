using ATMS.Infrastructure.Extensions;
using ATMS.Project.Services.Attachments;
using ATMS.Project.Services.Attachments.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class AttachmentsModule
{
    public static IServiceCollection AddAttachmentServices(
        this IServiceCollection services)
    {
        services.AddLocalFileStorage();
        services.AddSingleton<IAttachmentFileNameService, AttachmentFileNameService>();

        return services;
    }
}
