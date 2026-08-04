using ATMS.Data.Constants;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkProjects;
using Moq;

namespace Project.Services.Tests.Handlers.WorkProjects;

public class CreateWorkProjectHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenCurrentUserIsSuperAdmin_CreatesProjectWithCodeAndParticipants()
    {
        var currentUserId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var command = CreateCommand(participantId);
        var project = new WorkProject();
        CurrentUserMock.SetupGet(x => x.Id).Returns(currentUserId);
        CurrentUserMock.SetupGet(x => x.RoleId).Returns(RoleIds.SuperAdmin);
        MapperMock.Setup(x => x.Map<WorkProject>(command)).Returns(project);
        EntityCodeGeneratorMock.Setup(x => x.GetNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync("42");
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(project.Id, result);
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal("42", project.Code);
        Assert.Equal(currentUserId, project.CreatedById);
        var participant = Assert.Single(project.WorkProjectParticipants);
        Assert.Equal(participantId, participant.UserId);
        Assert.Equal(RoleIds.Developer, Assert.Single(participant.WorkProjectParticipantRoles).RoleId);
        WorkProjectRepositoryMock.Verify(
            x => x.CreateAsync(project, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private CreateWorkProjectHandler CreateHandler()
    {
        return new CreateWorkProjectHandler(
            CurrentUserMock.Object,
            MapperMock.Object,
            WorkProjectRepositoryMock.Object,
            EntityCodeGeneratorMock.Object);
    }

    private CreateWorkProjectCommand CreateCommand(Guid participantId)
    {
        return new CreateWorkProjectCommand
        {
            Title = " Project ",
            OrganizationId = Guid.NewGuid(),
            ProjectTypeId = 1,
            ProjectKindId = 1,
            ProjectStatusId = 1,
            Participants =
            [
                new WorkProjectParticipantCommand
                {
                    UserId = participantId,
                    RoleId = RoleIds.Developer
                }
            ]
        };
    }
}
