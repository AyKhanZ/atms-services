using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Notifications;
using ATMS.Project.Services.Notifications.Interfaces;
using Moq;

namespace Project.Services.Tests.Notifications;

public class WorkProjectNotificationServiceTest
{
    [Fact]
    public async Task NotifyParticipantsAddedAsync_SendsAddedToProjectFromTheCurrentUser()
    {
        var actorId = Guid.NewGuid();
        var userIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var project = new WorkProject { Id = Guid.NewGuid(), Title = "Project Alpha" };
        var notifications = new Mock<INotificationService>();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(user => user.Id).Returns(actorId);

        await new WorkProjectNotificationService(notifications.Object, currentUser.Object)
            .NotifyParticipantsAddedAsync(project, userIds, CancellationToken.None);

        notifications.Verify(service => service.AddAsync(
            It.Is<NotificationDraft>(draft =>
                draft.Type == NotificationTypeEnum.AddedToProject &&
                draft.ProjectId == project.Id &&
                draft.EntityType == NotificationEntityTypeEnum.Project &&
                draft.EntityId == project.Id &&
                draft.ActorId == actorId &&
                draft.Parameters.ProjectTitle == "Project Alpha" &&
                draft.Parameters.TaskCode == null),
            userIds,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
