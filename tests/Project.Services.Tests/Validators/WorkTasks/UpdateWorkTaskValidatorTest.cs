using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkTasks;
using Moq;

namespace Project.Services.Tests.Validators.WorkTasks;

public class UpdateWorkTaskValidatorTest
{
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid TaskId = Guid.NewGuid();
    private static readonly Guid TicketId = Guid.NewGuid();

    private readonly Mock<IWorkTaskRepository> _tasks = new();
    private readonly Mock<IDictionariesRepository> _dictionaries = new();

    public UpdateWorkTaskValidatorTest()
    {
        _dictionaries
            .Setup(x => x.IsWorkItemPriorityExistAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _dictionaries
            .Setup(x => x.IsWorkTaskStatusExistAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _tasks
            .Setup(x => x.IsWorkTicketExistAsync(ProjectId, TicketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private UpdateWorkTaskValidator Validator() =>
        new(_tasks.Object, _dictionaries.Object);

    private static UpdateWorkTaskCommand Command(Guid? parentId = null, Guid? ticketId = null) =>
        new()
        {
            ProjectId = ProjectId,
            WorkTaskId = TaskId,
            WorkTicketId = ticketId ?? TicketId,
            ParentWorkTaskId = parentId,
            Title = "Task",
            PriorityId = 1,
            StatusId = 1,
        };

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(999, false)]
    public async Task Validate_RequiresSupportedTaskStatus(int statusId, bool expectedValid)
    {
        _dictionaries
            .Setup(x => x.IsWorkTaskStatusExistAsync(statusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusId == 1);
        var command = Command();
        command.StatusId = statusId;

        var result = await Validator().ValidateAsync(command);

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public async Task Validate_RequiresAnExistingTicketWhenThereIsNoParent()
    {
        var result = await Validator().ValidateAsync(Command(ticketId: Guid.NewGuid()));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateWorkTaskCommand.WorkTicketId));
    }

    [Fact]
    public async Task Validate_AllowsAnUnknownTicketWhenAParentIsChosen()
    {
        // A subtask takes the parent's ticket, so whatever the client sent is irrelevant.
        var parentId = Guid.NewGuid();
        _tasks
            .Setup(x => x.FindParentAsync(ProjectId, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTask { Id = parentId, WorkTicketId = TicketId });

        var result = await Validator().ValidateAsync(Command(parentId, Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_RejectsTheTaskAsItsOwnParent()
    {
        var result = await Validator().ValidateAsync(Command(TaskId));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateWorkTaskCommand.ParentWorkTaskId));
        _tasks.Verify(
            x => x.FindParentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Validate_RejectsASubtaskAsParent()
    {
        var parentId = Guid.NewGuid();
        _tasks
            .Setup(x => x.FindParentAsync(ProjectId, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTask { Id = parentId, ParentWorkTaskId = Guid.NewGuid() });

        var result = await Validator().ValidateAsync(Command(parentId));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateWorkTaskCommand.ParentWorkTaskId));
    }

    [Fact]
    public async Task Validate_RejectsMovingATaskThatHasSubtasksUnderAParent()
    {
        var parentId = Guid.NewGuid();
        _tasks
            .Setup(x => x.FindParentAsync(ProjectId, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTask { Id = parentId, WorkTicketId = TicketId });
        _tasks
            .Setup(x => x.HasChildrenAsync(ProjectId, TaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await Validator().ValidateAsync(Command(parentId));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateWorkTaskCommand.ParentWorkTaskId));
    }
}
