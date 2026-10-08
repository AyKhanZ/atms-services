using ATMS.Infrastructure.Extensions;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Domain.Notifications;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddRequiredOptions<NotificationsOptions>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IWorkTaskNotificationService, WorkTaskNotificationService>();
        services.AddScoped<IWorkProjectNotificationService, WorkProjectNotificationService>();
        services.AddScoped<ICommentNotificationService, CommentNotificationService>();
        services.AddScoped<IDeadlineNotificationService, DeadlineNotificationService>();
        services.AddSingleton<DeliveryRetrySchedule>();
        services.AddHostedService<DeadlineNotificationBackgroundService>();
        services.AddHostedService<NotificationCleanupBackgroundService>();
        services.AddHostedService<NotificationEmailBackgroundService>();
        return services;
    }
}
