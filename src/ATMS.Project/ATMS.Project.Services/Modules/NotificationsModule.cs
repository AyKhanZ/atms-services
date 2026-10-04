using ATMS.Messaging.Infrastructure;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Notifications;
using ATMS.Project.Services.Notifications.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
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
