using ATMS.Application.Interfaces;
using ATMS.Application.Realtime;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Project.Services.Tests.Handlers.Notifications;

public sealed class MarkAllNotificationsReadHandlerTest
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IRealtimeEventPublisher> _publisher = new();

    public MarkAllNotificationsReadHandlerTest() => _currentUser.SetupGet(user => user.Id).Returns(_userId);

    private MarkAllNotificationsReadHandler Handler() => new(
        _repository.Object,
        _currentUser.Object,
        _publisher.Object,
        NullLogger<MarkAllNotificationsReadHandler>.Instance);

    [Fact]
    public async Task Handle_WhenRowsWereUpdated_PublishesTheCurrentCountAfterTheUpdate()
    {
        var updated = false;
        _repository.Setup(repository => repository.MarkAllReadAsync(
                _userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback(() => updated = true)
            .ReturnsAsync(2);
        _repository.Setup(repository => repository.CountUnreadAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => updated ? 1 : -1);

        await Handler().Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        _publisher.Verify(publisher => publisher.PublishToUsersAsync(
            It.Is<IEnumerable<Guid>>(users => users.Single() == _userId),
            RealtimeEventNames.NotificationRead,
            It.Is<NotificationReadEvent>(message => message.UnreadCount == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEverythingIsRead_DoesNotCountOrPublish()
    {
        _repository.Setup(repository => repository.MarkAllReadAsync(
                _userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        await Handler().Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        _repository.Verify(repository => repository.CountUnreadAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTheUpdateFails_DoesNotPublish()
    {
        _repository.Setup(repository => repository.MarkAllReadAsync(
                _userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Handler().Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None));

        _publisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenThePushFails_TheUpdateStillSucceeds()
    {
        _repository.Setup(repository => repository.MarkAllReadAsync(
                _userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publisher.Setup(publisher => publisher.PublishToUsersAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string>(),
                It.IsAny<NotificationReadEvent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SignalR unavailable"));

        await Handler().Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        _repository.Verify(repository => repository.MarkAllReadAsync(
            _userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTheRequestIsCancelledAfterTheUpdate_StillTellsTheOtherTabs()
    {
        using var request = new CancellationTokenSource();
        _repository.Setup(repository => repository.MarkAllReadAsync(
                _userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2)
            .Callback(request.Cancel);

        await Handler().Handle(new MarkAllNotificationsReadCommand(), request.Token);

        _publisher.Verify(publisher => publisher.PublishToUsersAsync(
            It.Is<IEnumerable<Guid>>(userIds => userIds.Single() == _userId),
            RealtimeEventNames.NotificationRead,
            It.IsAny<NotificationReadEvent>(),
            CancellationToken.None), Times.Once);
    }
}
