using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkGroups;
using Moq;

namespace Project.Services.Tests.Handlers.WorkGroups;

public class UpdateWorkGroupHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenItemExists_RenamesItWithoutMovingIt()
    {
        var projectId = Guid.NewGuid();
        var workGroupId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var entity = new WorkGroup
        {
            Id = workGroupId,
            WorkProjectId = projectId,
            ParentWorkGroupId = parentId,
            Title = "Old"
        };
        WorkGroupRepositoryMock
            .Setup(x => x.FindAsync(projectId, workGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var handler = CreateHandler();

        await handler.Handle(new UpdateWorkGroupCommand
        {
            ProjectId = projectId,
            WorkGroupId = workGroupId,
            Title = "  New name  "
        }, CancellationToken.None);

        Assert.Equal("New name", entity.Title);
        Assert.Equal(parentId, entity.ParentWorkGroupId);
        WorkGroupRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private UpdateWorkGroupHandler CreateHandler()
    {
        return new UpdateWorkGroupHandler(
            WorkGroupRepositoryMock.Object);
    }
}
