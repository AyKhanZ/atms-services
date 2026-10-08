using ATMS.Data.Enums;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.History;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Domain.Comments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Project.Services.Tests.Infrastructure;

public class NotificationEmailBackgroundServiceTest
{
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid TicketId = Guid.NewGuid();
    private static readonly Guid TaskId = Guid.NewGuid();
    private static readonly Guid RecipientUserId = Guid.NewGuid();

    private readonly Mock<IEmailDeliveryRepository> _deliveries = new();
    private readonly Mock<INotificationRepository> _notifications = new();
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<IEmailSender> _sender = new();
    private readonly EmailDelivery _delivery = new()
    {
        Id = Guid.NewGuid(),
        NotificationId = Guid.NewGuid(),
        Status = (int)DeliveryStatusEnum.Pending
    };

    public NotificationEmailBackgroundServiceTest()
    {
        _deliveries.Setup(repository => repository.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([_delivery]);
        _deliveries.Setup(repository => repository.GetAsync(_delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailDeliveryRow(
                _delivery.Id,
                (int)DeliveryStatusEnum.Pending,
                _delivery.NotificationId,
                RecipientUserId,
                "aykhan@baim.az",
                "Aykhan",
                "Zeynalov"));
    }

    private void Notification(NotificationTypeEnum type, bool entityDeleted = false, bool commentDeleted = false) =>
        _notifications.Setup(repository => repository.GetRowAsync(_delivery.NotificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRow
            {
                Id = _delivery.NotificationId,
                Type = (int)type,
                Actor = new HistoryPerson(Guid.NewGuid(), "Leyla", "Mammadova", null),
                WorkProjectId = ProjectId,
                EntityType = type == NotificationTypeEnum.AddedToProject
                    ? (int)NotificationEntityTypeEnum.Project
                    : (int)NotificationEntityTypeEnum.WorkTask,
                EntityId = type == NotificationTypeEnum.AddedToProject ? ProjectId : TaskId,
                WorkTicketId = TicketId,
                CommentId = type == NotificationTypeEnum.Mentioned ? Guid.Parse("11111111-1111-1111-1111-111111111111") : null,
                Parameters = new NotificationParameters
                {
                    ProjectTitle = "Project Alpha",
                    TaskCode = "41",
                    TaskTitle = "Payment form",
                    TaskKind = (int)WorkTaskKindEnum.Task,
                    Deadline = new DateOnly(2026, 10, 5)
                },
                EntityDeleted = entityDeleted,
                CommentDeleted = commentDeleted
            });

    private TestNotificationEmailBackgroundService Worker(bool sendEmails = true)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_deliveries.Object);
        services.AddSingleton(_notifications.Object);
        services.AddSingleton(_comments.Object);
        services.AddSingleton(_sender.Object);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NotificationsOptions:AppUrl"] = "http://localhost:4200/",
                ["NotificationsOptions:SendEmails"] = sendEmails.ToString()
            })
            .Build();
        return new TestNotificationEmailBackgroundService(
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            configuration);
    }

    [Fact]
    public async Task ProcessBatchAsync_SendsTheAssignmentWithALinkToTheTaskAndMarksItProcessed()
    {
        Notification(NotificationTypeEnum.TaskAssigned);

        var count = await Worker().ProcessOnceAsync(CancellationToken.None);

        Assert.Equal(1, count);
        _sender.Verify(sender => sender.SendAsync(
            "aykhan@baim.az",
            It.Is<TaskAssignedModel>(model =>
                model.Name == "Aykhan" &&
                model.ActorName == "Leyla Mammadova" &&
                model.TaskLabel == "TASK #41 Payment form" &&
                model.ProjectTitle == "Project Alpha" &&
                model.Link == $"http://localhost:4200/projects/{ProjectId}/tickets/{TicketId}/tasks/{TaskId}"),
            It.IsAny<CancellationToken>()), Times.Once);
        _deliveries.Verify(repository => repository.MarkProcessedAsync(_delivery.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenATitleHasALineBreak_PutsItOnOneLine()
    {
        // The subject is built from the label, and SMTP refuses a subject with a line break in it.
        _notifications.Setup(repository => repository.GetRowAsync(_delivery.NotificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRow
            {
                Id = _delivery.NotificationId,
                Type = (int)NotificationTypeEnum.TaskAssigned,
                WorkProjectId = ProjectId,
                EntityType = (int)NotificationEntityTypeEnum.WorkTask,
                EntityId = TaskId,
                WorkTicketId = TicketId,
                Parameters = new NotificationParameters
                {
                    ProjectTitle = "Project\nAlpha",
                    TaskCode = "41",
                    TaskTitle = "Payment\r\nform   v2"
                }
            });

        await Worker().ProcessOnceAsync(CancellationToken.None);

        _sender.Verify(sender => sender.SendAsync(
            It.IsAny<string>(),
            It.Is<TaskAssignedModel>(model =>
                model.TaskLabel == "TASK #41 Payment form v2" &&
                model.ProjectTitle == "Project Alpha" &&
                model.ActorName == "Someone"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_LeadsAMentionToTheComment()
    {
        Notification(NotificationTypeEnum.Mentioned);
        _comments.Setup(repository => repository.FindAsync(ProjectId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment { Text = $"Hello @[user:{RecipientUserId}]" });

        await Worker().ProcessOnceAsync(CancellationToken.None);

        _sender.Verify(sender => sender.SendAsync(
            It.IsAny<string>(),
            It.Is<MentionedModel>(model => model.Link.EndsWith("#comment-11111111-1111-1111-1111-111111111111")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenMentionWasRemoved_SkipsTheEmailAndMarksItProcessed()
    {
        Notification(NotificationTypeEnum.Mentioned);
        _comments.Setup(repository => repository.FindAsync(ProjectId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment { Text = "Hello" });

        await Worker().ProcessOnceAsync(CancellationToken.None);

        _sender.VerifyNoOtherCalls();
        _deliveries.Verify(repository => repository.MarkProcessedAsync(_delivery.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_SaysWhenAnOverdueTaskWasDue()
    {
        Notification(NotificationTypeEnum.TaskOverdue);

        await Worker().ProcessOnceAsync(CancellationToken.None);

        _sender.Verify(sender => sender.SendAsync(
            It.IsAny<string>(),
            It.Is<TaskOverdueModel>(model => model.Deadline == "5 Oct 2026"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_LeadsAnAddedToProjectToTheProject()
    {
        Notification(NotificationTypeEnum.AddedToProject);

        await Worker().ProcessOnceAsync(CancellationToken.None);

        _sender.Verify(sender => sender.SendAsync(
            It.IsAny<string>(),
            It.Is<AddedToProjectModel>(model => model.Link == $"http://localhost:4200/projects/{ProjectId}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(NotificationTypeEnum.TaskAssigned, true, false)]
    [InlineData(NotificationTypeEnum.Mentioned, false, true)]
    public async Task ProcessBatchAsync_WhenWhatItPointsToIsGone_SendsNothingAndIsDone(
        NotificationTypeEnum type,
        bool entityDeleted,
        bool commentDeleted)
    {
        Notification(type, entityDeleted, commentDeleted);

        await Worker().ProcessOnceAsync(CancellationToken.None);

        _sender.VerifyNoOtherCalls();
        _deliveries.Verify(repository => repository.MarkProcessedAsync(_delivery.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenSmtpFails_SchedulesARetry()
    {
        Notification(NotificationTypeEnum.DueToday);
        _sender.Setup(sender => sender.SendAsync(
                It.IsAny<string>(), It.IsAny<DueTodayModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        await Worker().ProcessOnceAsync(CancellationToken.None);

        _deliveries.Verify(repository => repository.MarkRetryAsync(
            _delivery.Id,
            1,
            It.IsAny<DateTime>(),
            "SMTP unavailable",
            It.IsAny<CancellationToken>()), Times.Once);
        _deliveries.Verify(repository => repository.MarkProcessedAsync(_delivery.Id, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailsAreOff_SendsNothingEvenFromTheQueue()
    {
        Notification(NotificationTypeEnum.TaskAssigned);
        var worker = Worker(sendEmails: false);

        await worker.StartAsync(CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        _deliveries.Verify(repository => repository.ClaimPendingAsync(
            It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _sender.VerifyNoOtherCalls();
    }

    private sealed class TestNotificationEmailBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
        : NotificationEmailBackgroundService(
            scopeFactory,
            new DeliveryRetrySchedule(),
            new CommentMentionService(),
            configuration,
            NullLogger<NotificationEmailBackgroundService>.Instance)
    {
        public Task<int> ProcessOnceAsync(CancellationToken cancellationToken) => ProcessBatchAsync(cancellationToken);
    }
}
