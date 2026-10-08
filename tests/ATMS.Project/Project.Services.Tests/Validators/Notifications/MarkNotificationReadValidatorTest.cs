using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.Notifications;
using Moq;

namespace Project.Services.Tests.Validators.Notifications;

public sealed class MarkNotificationReadValidatorTest : BaseValidatorTest
{
    private readonly Mock<INotificationRepository> _notifications = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _notificationId = Guid.NewGuid();

    public MarkNotificationReadValidatorTest()
    {
        _currentUser.SetupGet(user => user.Id).Returns(_userId);
        _notifications.Setup(repository => repository.IsExistAsync(
                _userId, _notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private MarkNotificationReadValidator Validator() => new(_notifications.Object, _currentUser.Object);

    [Fact]
    public async Task OwnNotification_IsValid()
    {
        var result = await Validator().ValidateAsync(new MarkNotificationReadCommand { NotificationId = _notificationId });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task EmptyId_UsesRequiredMessageAndDoesNotLookItUp()
    {
        var result = await Validator().ValidateAsync(new MarkNotificationReadCommand { NotificationId = Guid.Empty });

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(MarkNotificationReadCommand.NotificationId), error.PropertyName);
        Assert.Equal("Choose a notification.", error.ErrorMessage);
        _notifications.Verify(repository => repository.IsExistAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MissingOrSomeoneElsesNotification_IsRejected()
    {
        // The lookup is among the caller's own, so another person's id is simply not found.
        var result = await Validator().ValidateAsync(new MarkNotificationReadCommand { NotificationId = Guid.NewGuid() });

        var error = Assert.Single(result.Errors);
        Assert.Equal("This notification is no longer available. Refresh the list.", error.ErrorMessage);
    }
}
