using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Requests.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Notifications;
using Moq;

namespace Project.Services.Tests.Handlers.Notifications;

public sealed class GetNotificationSummaryHandlerTest
{
    [Fact]
    public async Task Handle_CountsTheCallersUnreadNotifications()
    {
        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(user => user.Id).Returns(userId);
        var repository = new Mock<INotificationRepository>();
        repository.Setup(notifications => notifications.CountUnreadAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var summary = await new GetNotificationSummaryHandler(repository.Object, currentUser.Object)
            .Handle(new GetNotificationSummaryRequest(), CancellationToken.None);

        Assert.Equal(7, summary.UnreadCount);
    }
}
