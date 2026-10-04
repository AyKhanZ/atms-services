using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Notifications;
using ATMS.Project.Services.Notifications.Interfaces;
using Moq;

namespace Project.Services.Tests.Notifications;

public class WorkTaskNotificationServiceTest
{
    private static readonly Guid ActorId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid AssigneeUserId = Guid.NewGuid();
    private static readonly Guid AssigneeParticipantId = Guid.NewGuid();

    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly List<(NotificationDraft Draft, Guid[] Recipients)> _sent = [];

    public WorkTaskNotificationServiceTest()
    {
        _currentUser.SetupGet(user => user.Id).Returns(ActorId);
        _repository
            .Setup(repository => repository.GetWorkTaskAudienceAsync(
                It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, Guid? assigneeId, CancellationToken _) =>
                new WorkTaskAudienceRow("Project Alpha", assigneeId == AssigneeParticipantId ? AssigneeUserId : null));
        _notifications
            .Setup(service => service.AddRangeAsync(
                It.IsAny<IReadOnlyCollection<NotificationRecipients>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<NotificationRecipients>, CancellationToken>((batch, _) =>
                _sent.AddRange(batch.Select(item => (item.Draft, item.UserIds.ToArray()))))
            .Returns(Task.CompletedTask);
    }

    private WorkTaskNotificationService Service() => new(_repository.Object, _notifications.Object, _currentUser.Object);

    private static WorkTask NewTask(Guid? assigneeId, int statusId, Guid? parentId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            WorkProjectId = Guid.NewGuid(),
            Code = "41",
            Title = "Payment form",
            AssigneeId = assigneeId,
            StatusId = statusId,
            ParentWorkTaskId = parentId,
            CreatedById = AuthorId
        };

    [Fact]
    public async Task NotifyChangedAsync_WhenANewTaskIsAssigned_TellsTheAssignee()
    {
        var task = NewTask(AssigneeParticipantId, (int)WorkTaskStatusEnum.New);

        await Service().NotifyChangedAsync(task, null, null, CancellationToken.None);

        var (draft, recipients) = Assert.Single(_sent);
        Assert.Equal(NotificationTypeEnum.TaskAssigned, draft.Type);
        Assert.Equal(task.WorkProjectId, draft.ProjectId);
        Assert.Equal(NotificationEntityTypeEnum.WorkTask, draft.EntityType);
        Assert.Equal(task.Id, draft.EntityId);
        Assert.Equal(ActorId, draft.ActorId);
        Assert.Equal("Project Alpha", draft.Parameters.ProjectTitle);
        Assert.Equal("41", draft.Parameters.TaskCode);
        Assert.Equal("Payment form", draft.Parameters.TaskTitle);
        Assert.Equal((int)WorkTaskKindEnum.Task, draft.Parameters.TaskKind);
        Assert.Equal([AssigneeUserId], recipients);
    }

    [Fact]
    public async Task NotifyChangedAsync_WhenTheTaskIsASubtask_SaysSo()
    {
        var task = NewTask(AssigneeParticipantId, (int)WorkTaskStatusEnum.New, parentId: Guid.NewGuid());

        await Service().NotifyChangedAsync(task, null, null, CancellationToken.None);

        Assert.Equal((int)WorkTaskKindEnum.Subtask, Assert.Single(_sent).Draft.Parameters.TaskKind);
    }

    [Fact]
    public async Task NotifyChangedAsync_WhenNothingItCaresAboutChanged_ReadsNothing()
    {
        var task = NewTask(AssigneeParticipantId, (int)WorkTaskStatusEnum.InProgress);

        await Service().NotifyChangedAsync(task, AssigneeParticipantId, (int)WorkTaskStatusEnum.InProgress, CancellationToken.None);

        Assert.Empty(_sent);
        _repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task NotifyChangedAsync_WhenANewTaskHasNoAssignee_SendsNothing()
    {
        await Service().NotifyChangedAsync(NewTask(null, (int)WorkTaskStatusEnum.New), null, null, CancellationToken.None);

        Assert.Empty(_sent);
    }

    [Fact]
    public async Task NotifyChangedAsync_WhenTheAssigneeIsRemoved_SendsNothing()
    {
        var task = NewTask(null, (int)WorkTaskStatusEnum.New);

        await Service().NotifyChangedAsync(task, AssigneeParticipantId, (int)WorkTaskStatusEnum.New, CancellationToken.None);

        Assert.Empty(_sent);
    }

    [Fact]
    public async Task NotifyChangedAsync_WhenTheStatusChanges_TellsTheAssigneeAndTheAuthor()
    {
        var task = NewTask(AssigneeParticipantId, (int)WorkTaskStatusEnum.Done);

        await Service().NotifyChangedAsync(task, AssigneeParticipantId, (int)WorkTaskStatusEnum.InProgress, CancellationToken.None);

        var (draft, recipients) = Assert.Single(_sent);
        Assert.Equal(NotificationTypeEnum.TaskStatusChanged, draft.Type);
        Assert.Equal((int)WorkTaskStatusEnum.InProgress, draft.Parameters.FromStatusId);
        Assert.Equal((int)WorkTaskStatusEnum.Done, draft.Parameters.ToStatusId);
        Assert.Equal([AssigneeUserId, AuthorId], recipients);
    }

    [Fact]
    public async Task NotifyChangedAsync_WhenAnUnassignedTaskChangesStatus_TellsOnlyTheAuthor()
    {
        var task = NewTask(null, (int)WorkTaskStatusEnum.Done);

        await Service().NotifyChangedAsync(task, null, (int)WorkTaskStatusEnum.New, CancellationToken.None);

        Assert.Equal([AuthorId], Assert.Single(_sent).Recipients);
    }

    [Fact]
    public async Task NotifyChangedAsync_WhenAssignedAndMovedAtOnce_TheNewAssigneeHearsOnlyTheAssignment()
    {
        var task = NewTask(AssigneeParticipantId, (int)WorkTaskStatusEnum.InProgress);

        await Service().NotifyChangedAsync(task, Guid.NewGuid(), (int)WorkTaskStatusEnum.New, CancellationToken.None);

        Assert.Equal(2, _sent.Count);
        Assert.Equal(NotificationTypeEnum.TaskAssigned, _sent[0].Draft.Type);
        Assert.Equal([AssigneeUserId], _sent[0].Recipients);
        Assert.Equal(NotificationTypeEnum.TaskStatusChanged, _sent[1].Draft.Type);
        Assert.Equal([AuthorId], _sent[1].Recipients);
        // The assignment carries no status change of its own.
        Assert.Null(_sent[0].Draft.Parameters.ToStatusId);
    }

    [Fact]
    public async Task NotifyChangedAsync_WhenTheProjectIsGone_SendsNothing()
    {
        _repository
            .Setup(repository => repository.GetWorkTaskAudienceAsync(
                It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTaskAudienceRow?)null);

        await Service().NotifyChangedAsync(NewTask(AssigneeParticipantId, 1), null, null, CancellationToken.None);

        Assert.Empty(_sent);
    }
}
