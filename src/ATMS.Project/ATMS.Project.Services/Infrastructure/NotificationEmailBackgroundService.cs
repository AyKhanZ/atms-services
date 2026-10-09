using Microsoft.Extensions.Options;
using System.Globalization;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Enums;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Comments.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Infrastructure;

// data is read at send time, so an email about a deleted task is not sent
public class NotificationEmailBackgroundService(
    IServiceScopeFactory scopeFactory,
    DeliveryRetrySchedule retrySchedule,
    ICommentMentionService mentions,
    IOptions<NotificationsOptions> notificationsOptions,
    ILogger<NotificationEmailBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan EmptyQueueDelay = TimeSpan.FromSeconds(5);

    private readonly NotificationsOptions _options = notificationsOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // emails off = nothing leaves, not even the queue (saves the test smtp limit)
        if (!_options.SendEmails)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedCount = await ProcessBatchAsync(stoppingToken);
                if (processedCount == 0)
                {
                    await Task.Delay(EmptyQueueDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification email worker failed while loading a delivery batch");
                await Task.Delay(EmptyQueueDelay, stoppingToken);
            }
        }
    }

    protected virtual async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        Data.Entities.EmailDelivery[] deliveries;
        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            deliveries = await scope.ServiceProvider
                .GetRequiredService<IEmailDeliveryRepository>()
                .ClaimPendingAsync(BatchSize, cancellationToken);
        }

        foreach (var delivery in deliveries)
        {
            await ProcessDeliveryAsync(delivery.Id, delivery.AttemptCount, cancellationToken);
        }

        return deliveries.Length;
    }

    private async Task ProcessDeliveryAsync(Guid deliveryId, int previousAttemptCount, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var deliveries = scope.ServiceProvider.GetRequiredService<IEmailDeliveryRepository>();
            var delivery = await deliveries.GetAsync(deliveryId, cancellationToken);
            if (delivery is null || delivery.Status != (int)DeliveryStatusEnum.Pending)
            {
                return;
            }

            var notification = await scope.ServiceProvider
                .GetRequiredService<INotificationRepository>()
                .GetRowAsync(delivery.NotificationId, cancellationToken);

            // inactive, deleted or the work is gone: mark done, a retry won't help
            if (notification is not null &&
                delivery.RecipientIsActive &&
                delivery.RecipientEmail is { } email &&
                !IsGone(notification))
            {
                var mentionStillPresent = true;
                if (notification.Type == (int)NotificationTypeEnum.Mentioned)
                {
                    var comment = notification.CommentId is { } commentId
                        ? await scope.ServiceProvider.GetRequiredService<ICommentRepository>()
                            .FindAsync(notification.WorkProjectId, commentId, cancellationToken)
                        : null;
                    mentionStillPresent = comment is not null &&
                        mentions.GetMentionedUserIds(comment.Text).Contains(delivery.RecipientUserId);
                }

                if (mentionStillPresent)
                {
                    await SendAsync(
                        email,
                        delivery,
                        notification,
                        scope.ServiceProvider.GetRequiredService<IEmailSender>(),
                        cancellationToken);
                }
            }

            await deliveries.MarkProcessedAsync(deliveryId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await HandleFailureAsync(deliveryId, previousAttemptCount, exception, cancellationToken);
        }
    }

    private static bool IsGone(NotificationRow notification) =>
        notification.EntityDeleted ||
        (notification.Type == (int)NotificationTypeEnum.Mentioned && notification.CommentDeleted);

    private async Task HandleFailureAsync(
        Guid deliveryId,
        int previousAttemptCount,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var deliveries = scope.ServiceProvider.GetRequiredService<IEmailDeliveryRepository>();
        var delivery = await deliveries.GetAsync(deliveryId, cancellationToken);
        if (delivery is null || delivery.Status != (int)DeliveryStatusEnum.Pending)
        {
            return;
        }

        var attemptCount = previousAttemptCount + 1;
        logger.LogError(
            exception,
            "Notification email {DeliveryId} failed on attempt {AttemptCount}",
            deliveryId,
            attemptCount);

        if (attemptCount >= retrySchedule.MaxAttemptCount)
        {
            await deliveries.MarkFailedAsync(deliveryId, attemptCount, exception.Message, cancellationToken);
            return;
        }

        await deliveries.MarkRetryAsync(
            deliveryId,
            attemptCount,
            retrySchedule.GetNextAttemptAt(attemptCount),
            exception.Message,
            cancellationToken);
    }

    private Task SendAsync(
        string to,
        EmailDeliveryRow delivery,
        NotificationRow notification,
        IEmailSender sender,
        CancellationToken cancellationToken)
    {
        var name = delivery.RecipientName ?? string.Empty;
        var surname = delivery.RecipientSurname ?? string.Empty;
        var actor = notification.Actor is { } person ? OneLine($"{person.Name} {person.Surname}") : "Someone";
        var parameters = notification.Parameters;
        var projectTitle = OneLine(parameters.ProjectTitle ?? string.Empty);
        var taskLabel = TaskLabel(notification);
        var link = Link(notification);

        return (NotificationTypeEnum)notification.Type switch
        {
            NotificationTypeEnum.TaskAssigned => sender.SendAsync(
                to,
                new TaskAssignedModel
                {
                    Name = name,
                    Surname = surname,
                    ActorName = actor,
                    TaskLabel = taskLabel,
                    ProjectTitle = projectTitle,
                    Link = link
                },
                cancellationToken),
            NotificationTypeEnum.Mentioned => sender.SendAsync(
                to,
                new MentionedModel
                {
                    Name = name,
                    Surname = surname,
                    ActorName = actor,
                    TaskLabel = taskLabel,
                    ProjectTitle = projectTitle,
                    Link = link
                },
                cancellationToken),
            NotificationTypeEnum.DueToday => sender.SendAsync(
                to,
                new DueTodayModel
                {
                    Name = name,
                    Surname = surname,
                    TaskLabel = taskLabel,
                    ProjectTitle = projectTitle,
                    Link = link
                },
                cancellationToken),
            NotificationTypeEnum.TaskOverdue => sender.SendAsync(
                to,
                new TaskOverdueModel
                {
                    Name = name,
                    Surname = surname,
                    TaskLabel = taskLabel,
                    ProjectTitle = projectTitle,
                    Deadline = parameters.Deadline?.ToString("d MMM yyyy", CultureInfo.InvariantCulture) ?? string.Empty,
                    Link = link
                },
                cancellationToken),
            NotificationTypeEnum.AddedToProject => sender.SendAsync(
                to,
                new AddedToProjectModel
                {
                    Name = name,
                    Surname = surname,
                    ActorName = actor,
                    ProjectTitle = projectTitle,
                    Link = link
                },
                cancellationToken),
            _ => Task.CompletedTask
        };
    }

    // smtp refuses a subject with a line break, so collapse the whitespace
    private static string OneLine(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    // "TASK #41 Payment form", like the bell
    private static string TaskLabel(NotificationRow notification)
    {
        var parameters = notification.Parameters;
        var kind = parameters.TaskKind == (int)WorkTaskKindEnum.Subtask ? "SUBTASK" : "TASK";
        return OneLine($"{kind} #{parameters.TaskCode} {parameters.TaskTitle}");
    }

    // the same page the bell opens
    private string Link(NotificationRow notification)
    {
        var app = _options.AppUrl.TrimEnd('/');
        if (notification.EntityType == (int)NotificationEntityTypeEnum.Project)
        {
            return $"{app}/projects/{notification.EntityId}";
        }

        var task = $"{app}/projects/{notification.WorkProjectId}/tickets/{notification.WorkTicketId}/tasks/{notification.EntityId}";
        return notification.CommentId is { } commentId ? $"{task}#comment-{commentId}" : task;
    }
}
