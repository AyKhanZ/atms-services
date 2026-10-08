using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Services.Validation.WorkTasks;

namespace Project.Services.Tests.Validators.WorkTasks;

public class DeleteWorkTaskValidatorTest
{
    // Subtasks no longer block a delete: they go with the task (DeleteWorkTaskHandler).
    [Fact]
    public async Task Validate_AcceptsAnyTaskInTheProject()
    {
        var command = new DeleteWorkTaskCommand { ProjectId = Guid.NewGuid(), WorkTaskId = Guid.NewGuid() };

        var result = await new DeleteWorkTaskValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_RejectsMissingIdentifiers()
    {
        var result = await new DeleteWorkTaskValidator().ValidateAsync(new DeleteWorkTaskCommand());

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DeleteWorkTaskCommand.ProjectId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DeleteWorkTaskCommand.WorkTaskId));
    }
}
