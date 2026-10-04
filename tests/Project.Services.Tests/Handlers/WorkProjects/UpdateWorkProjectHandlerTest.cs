using ATMS.Data.Constants;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkProjects;
using ATMS.Project.Services.Security.Interfaces;
using Moq;

namespace Project.Services.Tests.Handlers.WorkProjects;

public class UpdateWorkProjectHandlerTest : BaseHandlerTest
{
    private readonly Mock<IProjectPermissionService> _permissions = new();

    private UpdateWorkProjectHandler Handler() => new(
        CurrentUserMock.Object,
        MapperMock.Object,
        WorkProjectRepositoryMock.Object,
        CacheServiceMock.Object,
        _permissions.Object,
        WorkProjectNotificationServiceMock.Object);

    [Fact]
    public async Task Handle_TellsOnlyThePeopleWhoWereNotInTheProjectYet()
    {
        var stayingUserId = Guid.NewGuid();
        var removedUserId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();
        var project = new WorkProject
        {
            Id = Guid.NewGuid(),
            WorkProjectParticipants =
            [
                Participant(stayingUserId),
                Participant(removedUserId)
            ]
        };
        WorkProjectRepositoryMock
            .Setup(repository => repository.FindAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        var steps = new List<string>();
        IEnumerable<Guid>? notified = null;
        WorkProjectNotificationServiceMock
            .Setup(service => service.NotifyParticipantsAddedAsync(
                project, It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<WorkProject, IEnumerable<Guid>, CancellationToken>((_, userIds, _) =>
            {
                notified = userIds.ToArray();
                steps.Add("notify");
            })
            .Returns(Task.CompletedTask);
        WorkProjectRepositoryMock
            .Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("save"))
            .Returns(Task.CompletedTask);

        await Handler().Handle(Command(project.Id, stayingUserId, newUserId), CancellationToken.None);

        Assert.Equal([newUserId], notified);
        Assert.Equal(["notify", "save"], steps);
    }

    [Fact]
    public async Task Handle_WhenNobodyIsAdded_PassesNoOne()
    {
        var userId = Guid.NewGuid();
        var project = new WorkProject { Id = Guid.NewGuid(), WorkProjectParticipants = [Participant(userId)] };
        WorkProjectRepositoryMock
            .Setup(repository => repository.FindAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        await Handler().Handle(Command(project.Id, userId), CancellationToken.None);

        WorkProjectNotificationServiceMock.Verify(service => service.NotifyParticipantsAddedAsync(
            project,
            It.Is<IEnumerable<Guid>>(userIds => !userIds.Any()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static WorkProjectParticipant Participant(Guid userId) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WorkProjectParticipantRoles = [new WorkProjectParticipantRole { RoleId = RoleIds.Developer }]
        };

    private static UpdateWorkProjectCommand Command(Guid projectId, params Guid[] userIds) =>
        new()
        {
            Id = projectId,
            Title = "Project",
            ProjectTypeId = 1,
            ProjectKindId = 1,
            ProjectStatusId = 1,
            Participants = userIds
                .Select(userId => new WorkProjectParticipantCommand { UserId = userId, RoleId = RoleIds.Developer })
                .ToArray()
        };
}
