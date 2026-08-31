using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Constants;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.WorkProjects;
using ATMS.Project.Services.Security.Interfaces;
using Moq;

namespace Project.Services.Tests.Handlers.WorkProjects;

public class AddWorkProjectParticipantHandlerTest
{
    private readonly Mock<IWorkProjectRepository> workProjectRepository = new();
    private readonly Mock<ICacheService> cache = new();
    private readonly Mock<IProjectPermissionService> projectPermissionService = new();
    [Fact]
    public async Task Handle_WhenClientInvitePermissionMatchesTarget_AddsParticipant()
    {
        var command = CreateCommand(RoleIds.OrgClientViewer);
        var project = new WorkProject { Id = command.ProjectId };
        workProjectRepository
            .Setup(repository => repository.FindAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        var participant = Assert.Single(project.WorkProjectParticipants);
        Assert.Equal(command.UserId, participant.UserId);
        Assert.Equal(command.RoleId, Assert.Single(participant.WorkProjectParticipantRoles).RoleId);
        workProjectRepository.Verify(
            repository => repository.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private AddWorkProjectParticipantHandler CreateHandler() => new(
        workProjectRepository.Object,
        cache.Object,
        projectPermissionService.Object);

    private static AddWorkProjectParticipantCommand CreateCommand(Guid roleId) => new()
    {
        ProjectId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        RoleId = roleId
    };
}
