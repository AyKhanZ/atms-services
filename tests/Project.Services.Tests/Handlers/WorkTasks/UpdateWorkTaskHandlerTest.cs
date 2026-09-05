using ATMS.Caching.Constants;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTasks;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTasks;

public class UpdateWorkTaskHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenTaskExists_SavesAndInvalidatesDetailsCache()
    {
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            WorkProjectId = Guid.NewGuid()
        };
        var childIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var command = new UpdateWorkTaskCommand { ProjectId = task.WorkProjectId, WorkTaskId = task.Id, Title = "Updated", PriorityId = 1, StatusId = 2 };
        WorkTaskRepositoryMock.Setup(repository => repository.FindAsync(task.WorkProjectId, task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetChildIdsAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childIds);
        var handler = new UpdateWorkTaskHandler(MapperMock.Object, WorkTaskRepositoryMock.Object, CacheServiceMock.Object);

        await handler.Handle(command, CancellationToken.None);

        MapperMock.Verify(mapper => mapper.Map(command, task), Times.Once);
        WorkTaskRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.TaskById(task.Id, language));
        foreach (var childId in childIds)
        {
            VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.TaskById(childId, language));
        }
    }
}
