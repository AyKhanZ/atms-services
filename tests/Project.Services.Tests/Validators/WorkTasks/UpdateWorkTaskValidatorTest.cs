using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkTasks;
using Moq;

namespace Project.Services.Tests.Validators.WorkTasks;

public class UpdateWorkTaskValidatorTest
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(999, false)]
    public async Task Validate_RequiresSupportedTaskStatus(int statusId, bool expectedValid)
    {
        var tasks = new Mock<IWorkTaskRepository>();
        var dictionaries = new Mock<IDictionariesRepository>();
        dictionaries.Setup(x => x.IsWorkItemPriorityExistAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        dictionaries.Setup(x => x.IsWorkTaskStatusExistAsync(statusId, It.IsAny<CancellationToken>())).ReturnsAsync(statusId == 1);
        var validator = new UpdateWorkTaskValidator(tasks.Object, dictionaries.Object);

        var result = await validator.ValidateAsync(new UpdateWorkTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkTaskId = Guid.NewGuid(),
            Title = "Task",
            PriorityId = 1,
            StatusId = statusId
        });

        Assert.Equal(expectedValid, result.IsValid);
    }
}
