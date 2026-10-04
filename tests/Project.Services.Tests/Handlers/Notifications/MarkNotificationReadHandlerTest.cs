using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Notifications;
using Moq;

namespace Project.Services.Tests.Handlers.Notifications;

public sealed class MarkNotificationReadHandlerTest
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    public MarkNotificationReadHandlerTest() => _currentUser.SetupGet(user => user.Id).Returns(_userId);

    private MarkNotificationReadHandler Handler() => new(_repository.Object, _currentUser.Object);

    private Notification Found(DateTime? readAt)
    {
        var notification = new Notification { Id = Guid.NewGuid(), UserId = _userId, ReadAt = readAt };
        _repository.Setup(repository => repository.FindAsync(_userId, notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        return notification;
    }

    [Fact]
    public async Task Handle_WhenUnread_MarksItReadAndSaves()
    {
        var notification = Found(readAt: null);
        var before = DateTime.UtcNow;

        await Handler().Handle(new MarkNotificationReadCommand { NotificationId = notification.Id }, CancellationToken.None);

        Assert.NotNull(notification.ReadAt);
        Assert.InRange(notification.ReadAt.Value, before, DateTime.UtcNow);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlreadyRead_KeepsTheFirstReadTimeAndSavesNothing()
    {
        var readAt = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var notification = Found(readAt);

        await Handler().Handle(new MarkNotificationReadCommand { NotificationId = notification.Id }, CancellationToken.None);

        Assert.Equal(readAt, notification.ReadAt);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTheNotificationIsNotTheCallers_IsNotFound()
    {
        // The repository looks only among the caller's own notifications: someone else's is not found.
        var exception = await Assert.ThrowsAsync<EntityException>(() => Handler().Handle(
            new MarkNotificationReadCommand { NotificationId = Guid.NewGuid() },
            CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
