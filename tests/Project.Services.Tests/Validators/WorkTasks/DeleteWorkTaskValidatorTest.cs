using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkTasks;
using Moq;

namespace Project.Services.Tests.Validators.WorkTasks;

public class DeleteWorkTaskValidatorTest
{
    private readonly Mock<IWorkTaskRepository> _tasks = new();

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Validate_RejectsTaskWhenItHasSubtasks(bool hasSubtasks, bool expectedValid)
    {
        var command = new DeleteWorkTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkTaskId = Guid.NewGuid()
        };
        _tasks.Setup(repository => repository.HasChildrenAsync(
                command.ProjectId,
                command.WorkTaskId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasSubtasks);

        var result = await new DeleteWorkTaskValidator(_tasks.Object).ValidateAsync(command);

        Assert.Equal(expectedValid, result.IsValid);
        if (hasSubtasks)
        {
            Assert.Contains(result.Errors, error =>
                error.PropertyName == nameof(command.WorkTaskId) &&
                error.ErrorMessage.Contains("subtask", StringComparison.OrdinalIgnoreCase));
        }
    }
}
