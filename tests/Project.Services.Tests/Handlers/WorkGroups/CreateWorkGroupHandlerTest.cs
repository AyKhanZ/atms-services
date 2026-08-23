using System.Linq.Expressions;
using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkGroups;
using Moq;

namespace Project.Services.Tests.Handlers.WorkGroups;

public class CreateWorkGroupHandlerTest : BaseHandlerTest
{
    public CreateWorkGroupHandlerTest()
    {
        WorkProjectRepositoryMock
            .Setup(x => x.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Handle_WhenCreatingRootGroup_TrimsNameAndCreatesPlannedItem()
    {
        var currentUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        WorkGroup? created = null;
        CurrentUserMock.SetupGet(x => x.Id).Returns(currentUserId);
        WorkGroupRepositoryMock
            .Setup(x => x.CreateAsync(
                It.IsAny<WorkGroup>(),
                It.IsAny<CancellationToken>()))
            .Callback<WorkGroup, CancellationToken>((entity, _) => created = entity);
        var handler = CreateHandler();

        var id = await handler.Handle(new CreateWorkGroupCommand
        {
            ProjectId = projectId,
            Title = "  Group name  "
        }, CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(id, created.Id);
        Assert.Equal("Group name", created.Title);
        Assert.Equal(projectId, created.WorkProjectId);
        Assert.Null(created.ParentWorkGroupId);
        Assert.Equal((int)WorkGroupStatusEnum.Planned, created.StatusId);
        Assert.Equal(currentUserId, created.CreatedById);
    }

    [Fact]
    public async Task Handle_WhenCreatingMilestone_RequiresRootParentFromSameProject()
    {
        var projectId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        WorkGroupRepositoryMock
            .Setup(x => x.IsRootExistAsync(projectId, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = CreateHandler();

        await handler.Handle(new CreateWorkGroupCommand
        {
            ProjectId = projectId,
            ParentWorkGroupId = parentId,
            Title = "Milestone"
        }, CancellationToken.None);

        WorkGroupRepositoryMock.Verify(
            x => x.IsRootExistAsync(projectId, parentId, It.IsAny<CancellationToken>()),
            Times.Once);
        WorkGroupRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<WorkGroup>(entity => entity.ParentWorkGroupId == parentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenParentIsNotARootInSameProject_ThrowsNotFound()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<EntityException>(() => handler.Handle(
            new CreateWorkGroupCommand
            {
                ProjectId = Guid.NewGuid(),
                ParentWorkGroupId = Guid.NewGuid(),
                Title = "Milestone"
            },
            CancellationToken.None));
    }

    private CreateWorkGroupHandler CreateHandler()
    {
        return new CreateWorkGroupHandler(
            CurrentUserMock.Object,
            WorkProjectRepositoryMock.Object,
            WorkGroupRepositoryMock.Object);
    }
}
