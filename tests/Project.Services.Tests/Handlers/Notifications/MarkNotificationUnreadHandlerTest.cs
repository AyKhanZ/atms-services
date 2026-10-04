using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Notifications;
using Moq;

namespace Project.Services.Tests.Handlers.Notifications;

public sealed class MarkNotificationUnreadHandlerTest
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    public MarkNotificationUnreadHandlerTest() => _currentUser.SetupGet(user => user.Id).Returns(_userId);

    private MarkNotificationUnreadHandler Handler() => new(_repository.Object, _currentUser.Object);

    private Notification Found(DateTime? readAt)
    {
        var notification = new Notification { Id = Guid.NewGuid(), UserId = _userId, ReadAt = readAt };
        _repository.Setup(repository => repository.FindAsync(_userId, notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        return notification;
    }

    [Fact]
    public async Task Handle_WhenRead_MarksItUnreadAndSaves()
    {
        var notification = Found(DateTime.UtcNow);

        await Handler().Handle(new MarkNotificationUnreadCommand { NotificationId = notification.Id }, CancellationToken.None);

        Assert.Null(notification.ReadAt);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlreadyUnread_SavesNothing()
    {
        var notification = Found(readAt: null);

        await Handler().Handle(new MarkNotificationUnreadCommand { NotificationId = notification.Id }, CancellationToken.None);

        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTheNotificationIsNotTheCallers_IsNotFound()
    {
        var exception = await Assert.ThrowsAsync<EntityException>(() => Handler().Handle(
            new MarkNotificationUnreadCommand { NotificationId = Guid.NewGuid() },
            CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}
