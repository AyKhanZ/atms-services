using ATMS.Project.Services.Board;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTasks;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTasks;

public class CreateWorkTaskHandlerTest : BaseHandlerTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_CreatesTaskWithServerDerivedHierarchyAndNewStatus(bool hasParent)
    {
        var command = new CreateWorkTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkTicketId = Guid.NewGuid(),
            ParentWorkTaskId = hasParent ? Guid.NewGuid() : null,
            Title = "Task",
            PriorityId = 1
        };
        var parentTicketId = Guid.NewGuid();
        if (command.ParentWorkTaskId.HasValue)
        {
            WorkTaskRepositoryMock
                .Setup(repository => repository.FindParentAsync(
                    command.ProjectId,
                    command.ParentWorkTaskId.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WorkTask
                {
                    Id = command.ParentWorkTaskId.Value,
                    ParentWorkTaskId = null,
                    WorkTicketId = parentTicketId
                });
        }
        var entity = new WorkTask();
        WorkTaskRepositoryMock
            .Setup(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        MapperMock.Setup(mapper => mapper.Map<WorkTask>(command)).Returns(entity);
        EntityCodeGeneratorMock.Setup(generator => generator.GetNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync("42");
        var handler = new CreateWorkTaskHandler(
            MapperMock.Object,
            WorkTaskRepositoryMock.Object,
            EntityCodeGeneratorMock.Object,
            new WorkTaskBoardPlacementService(WorkTaskRepositoryMock.Object, new WorkTaskBoardPositionService()));

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(id, entity.Id);
        Assert.Equal("42", entity.Code);
        Assert.Equal((int)WorkTaskStatusEnum.New, entity.StatusId);
        Assert.Equal(hasParent ? parentTicketId : command.WorkTicketId, entity.WorkTicketId);
        Assert.False(string.IsNullOrEmpty(entity.Rank));
        WorkTaskRepositoryMock.Verify(repository => repository.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        WorkTaskRepositoryMock.Verify(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
