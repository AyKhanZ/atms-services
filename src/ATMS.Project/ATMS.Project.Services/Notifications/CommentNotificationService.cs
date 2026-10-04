using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Comments.Interfaces;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Notifications.Interfaces;

namespace ATMS.Project.Services.Notifications;

// Who hears about a comment. A mention outranks the comment itself, so a person named in it gets
// only Mentioned. The text is never copied into the notification: a deleted comment must not live
// on there.
public sealed class CommentNotificationService(
    INotificationRepository repository,
    INotificationService notifications,
    ICommentMentionService mentions,
    ICurrentUser currentUser) : ICommentNotificationService
{
    public async Task NotifyCreatedAsync(Guid projectId, Comment comment, CancellationToken cancellationToken)
    {
        var task = await repository.GetCommentedTaskAsync(projectId, comment.OwnerId, cancellationToken);
        if (task is null)
        {
            return;
        }

        var mentioned = mentions.GetMentionedUserIds(comment.Text);
        Guid?[] involved = [task.AssigneeUserId, task.AuthorId, .. task.CommenterIds.Cast<Guid?>()];
        var batch = new List<NotificationRecipients>
        {
            new(
                Draft(NotificationTypeEnum.CommentAdded, projectId, comment, task),
                involved.OfType<Guid>().Except(mentioned).ToArray())
        };
        if (mentioned.Length > 0)
        {
            batch.Insert(0, new NotificationRecipients(
                Draft(NotificationTypeEnum.Mentioned, projectId, comment, task),
                mentioned));
        }

        await notifications.AddRangeAsync(batch, cancellationToken);
    }

    public async Task NotifyEditedAsync(
        Guid projectId,
        Comment comment,
        string previousText,
        CancellationToken cancellationToken)
    {
        var newlyMentioned = mentions.GetMentionedUserIds(comment.Text)
            .Except(mentions.GetMentionedUserIds(previousText))
            .ToArray();
        if (newlyMentioned.Length == 0)
        {
            return;
        }

        var task = await repository.GetCommentedTaskAsync(projectId, comment.OwnerId, cancellationToken);
        if (task is null)
        {
            return;
        }

        await notifications.AddAsync(
            Draft(NotificationTypeEnum.Mentioned, projectId, comment, task),
            newlyMentioned,
            cancellationToken);
    }

    private NotificationDraft Draft(
        NotificationTypeEnum type,
        Guid projectId,
        Comment comment,
        CommentedTaskRow task) =>
        new(
            type,
            projectId,
            NotificationEntityTypeEnum.WorkTask,
            comment.OwnerId,
            new NotificationParameters
            {
                ProjectTitle = task.ProjectTitle,
                TaskCode = task.Code,
                TaskTitle = task.Title,
                TaskKind = (int)(task.IsSubtask ? WorkTaskKindEnum.Subtask : WorkTaskKindEnum.Task)
            })
        {
            ActorId = currentUser.Id,
            CommentId = comment.Id
        };
}
