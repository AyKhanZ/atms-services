using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Comments;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Domain.Notifications;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using Moq;

namespace Project.Services.Tests.Domain.Notifications;

public class CommentNotificationServiceTest
{
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid TaskId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();
    private static readonly Guid AssigneeId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid CommenterId = Guid.NewGuid();

    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly List<(NotificationDraft Draft, Guid[] Recipients)> _sent = [];

    public CommentNotificationServiceTest()
    {
        _currentUser.SetupGet(user => user.Id).Returns(ActorId);
        _repository
            .Setup(repository => repository.GetCommentedTaskAsync(ProjectId, TaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentedTaskRow(
                "Project Alpha",
                "41",
                "Payment form",
                true,
                AssigneeId,
                AuthorId,
                [CommenterId, ActorId]));
        _notifications
            .Setup(service => service.AddRangeAsync(
                It.IsAny<IReadOnlyCollection<NotificationRecipients>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<NotificationRecipients>, CancellationToken>((batch, _) =>
                _sent.AddRange(batch.Select(item => (item.Draft, item.UserIds.ToArray()))))
            .Returns(Task.CompletedTask);
        _notifications
            .Setup(service => service.AddAsync(
                It.IsAny<NotificationDraft>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationDraft, IEnumerable<Guid>, CancellationToken>(
                (draft, recipients, _) => _sent.Add((draft, recipients.ToArray())))
            .Returns(Task.CompletedTask);
    }

    private CommentNotificationService Service() =>
        new(_repository.Object, _notifications.Object, new CommentMentionService(), _currentUser.Object);

    private static Comment Comment(string text) =>
        new() { Id = Guid.NewGuid(), OwnerType = (int)CommentOwnerTypeEnum.Task, OwnerId = TaskId, Text = text };

    private static string Mention(Guid userId) => $"@[user:{userId}]";

    [Fact]
    public async Task NotifyCreatedAsync_TellsTheAssigneeTheAuthorAndEveryoneWhoCommented()
    {
        var comment = Comment("Checked on staging");

        await Service().NotifyCreatedAsync(ProjectId, comment, CancellationToken.None);

        var (draft, recipients) = Assert.Single(_sent);
        Assert.Equal(NotificationTypeEnum.CommentAdded, draft.Type);
        Assert.Equal(ProjectId, draft.ProjectId);
        Assert.Equal(NotificationEntityTypeEnum.WorkTask, draft.EntityType);
        Assert.Equal(TaskId, draft.EntityId);
        Assert.Equal(comment.Id, draft.CommentId);
        Assert.Equal(ActorId, draft.ActorId);
        Assert.Equal([AssigneeId, AuthorId, CommenterId, ActorId], recipients);
    }

    [Fact]
    public async Task NotifyCreatedAsync_NeverCopiesTheCommentText()
    {
        await Service().NotifyCreatedAsync(ProjectId, Comment("Secret text"), CancellationToken.None);

        var parameters = Assert.Single(_sent).Draft.Parameters;
        Assert.Equal(
            new NotificationParameters
            {
                ProjectTitle = "Project Alpha",
                TaskCode = "41",
                TaskTitle = "Payment form",
                TaskKind = (int)WorkTaskKindEnum.Subtask
            },
            parameters);
    }

    [Fact]
    public async Task NotifyCreatedAsync_WhenSomeoneIsMentioned_TheyGetOnlyMentioned()
    {
        var outsider = Guid.NewGuid();
        var comment = Comment($"{Mention(AssigneeId)} and {Mention(outsider)}, please look");

        await Service().NotifyCreatedAsync(ProjectId, comment, CancellationToken.None);

        Assert.Equal(2, _sent.Count);
        Assert.Equal(NotificationTypeEnum.Mentioned, _sent[0].Draft.Type);
        Assert.Equal(comment.Id, _sent[0].Draft.CommentId);
        Assert.Equal([AssigneeId, outsider], _sent[0].Recipients);
        Assert.Equal(NotificationTypeEnum.CommentAdded, _sent[1].Draft.Type);
        Assert.DoesNotContain(AssigneeId, _sent[1].Recipients);
    }

    [Fact]
    public async Task NotifyCreatedAsync_WhenTheTaskIsGone_SendsNothing()
    {
        var comment = Comment("Hello");
        comment.OwnerId = Guid.NewGuid();

        await Service().NotifyCreatedAsync(ProjectId, comment, CancellationToken.None);

        Assert.Empty(_sent);
    }

    [Fact]
    public async Task NotifyEditedAsync_TellsOnlyPeopleMentionedForTheFirstTime()
    {
        var alreadyMentioned = Guid.NewGuid();
        var newlyMentioned = Guid.NewGuid();
        var comment = Comment($"{Mention(alreadyMentioned)} {Mention(newlyMentioned)}");

        await Service().NotifyEditedAsync(ProjectId, comment, Mention(alreadyMentioned), CancellationToken.None);

        var (draft, recipients) = Assert.Single(_sent);
        Assert.Equal(NotificationTypeEnum.Mentioned, draft.Type);
        Assert.Equal(comment.Id, draft.CommentId);
        Assert.Equal([newlyMentioned], recipients);
    }

    [Theory]
    [InlineData("Fixed a typo")]
    [InlineData("@[user:00000000-0000-0000-0000-000000000001] still here")]
    public async Task NotifyEditedAsync_WhenNobodyNewIsMentioned_ReadsNothing(string text)
    {
        var previous = "Typo @[user:00000000-0000-0000-0000-000000000001]";

        await Service().NotifyEditedAsync(ProjectId, Comment(text), previous, CancellationToken.None);

        Assert.Empty(_sent);
        _repository.VerifyNoOtherCalls();
    }
}
