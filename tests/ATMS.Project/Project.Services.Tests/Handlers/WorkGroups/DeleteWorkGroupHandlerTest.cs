using ATMS.Application.Exceptions.Conflict;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkGroups;
using Moq;

namespace Project.Services.Tests.Handlers.WorkGroups;

public class DeleteWorkGroupHandlerTest : BaseHandlerTest
{
    public DeleteWorkGroupHandlerTest()
    {
        WorkGroupRepositoryMock
            .Setup(x => x.HasChildrenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        WorkGroupRepositoryMock
            .Setup(x => x.HasTicketsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task Handle_WhenItemIsEmpty_SoftDeletesIt()
    {
        var currentUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var entity = new WorkGroup { Id = Guid.NewGuid(), WorkProjectId = projectId };
        CurrentUserMock.SetupGet(x => x.Id).Returns(currentUserId);
        WorkGroupRepositoryMock
            .Setup(x => x.FindAsync(projectId, entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var handler = CreateHandler();

        await handler.Handle(new DeleteWorkGroupCommand
        {
            ProjectId = projectId,
            WorkGroupId = entity.Id
        }, CancellationToken.None);

        Assert.True(entity.IsDeleted);
        Assert.NotNull(entity.DeletedAt);
        Assert.Equal(currentUserId, entity.DeletedById);
        WorkGroupRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Handle_WhenItemIsNotEmpty_ThrowsConflict(
        bool hasChildren,
        bool hasTickets)
    {
        var entity = new WorkGroup { Id = Guid.NewGuid() };
        WorkGroupRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Guid>(),
                entity.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        WorkGroupRepositoryMock
            .Setup(x => x.HasChildrenAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasChildren);
        WorkGroupRepositoryMock
            .Setup(x => x.HasTicketsAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasTickets);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new DeleteWorkGroupCommand
            {
                ProjectId = Guid.NewGuid(),
                WorkGroupId = entity.Id
            },
            CancellationToken.None));

        Assert.False(entity.IsDeleted);
        WorkGroupRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private DeleteWorkGroupHandler CreateHandler()
    {
        return new DeleteWorkGroupHandler(
            CurrentUserMock.Object,
            WorkGroupRepositoryMock.Object);
    }
}
