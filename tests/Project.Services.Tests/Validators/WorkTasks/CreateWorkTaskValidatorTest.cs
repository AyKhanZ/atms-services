using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Validation.WorkTasks;
using Moq;

namespace Project.Services.Tests.Validators.WorkTasks;

public class CreateWorkTaskValidatorTest : BaseValidatorTest
{
    public CreateWorkTaskValidatorTest()
    {
        WorkTasksRepositoryMock.Setup(x => x.IsWorkTicketExistAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        WorkTasksRepositoryMock.Setup(x => x.IsProjectParticipantExistAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        WorkTasksRepositoryMock.Setup(x => x.IsStaffProjectParticipantExistAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        DictionariesRepositoryMock.Setup(x => x.IsWorkItemPriorityExistAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public async Task Validate_EnforcesTrimmedTitleBoundary(int length, bool expectedValid)
    {
        var command = ValidCommand();
        command.Title = length == 0 ? "Task" : new string('a', length);

        var result = await Validator().ValidateAsync(command);

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData(2000, true)]
    [InlineData(2001, false)]
    public async Task Validate_EnforcesDescriptionBoundary(int length, bool expectedValid)
    {
        var command = ValidCommand();
        command.Description = new string('a', length);

        var result = await Validator().ValidateAsync(command);

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenParentIsSubtask_FailsHierarchyValidation()
    {
        var command = ValidCommand();
        command.ParentWorkTaskId = Guid.NewGuid();
        WorkTasksRepositoryMock.Setup(x => x.FindParentAsync(command.ProjectId, command.ParentWorkTaskId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTask { ParentWorkTaskId = Guid.NewGuid() });

        var result = await Validator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.ParentWorkTaskId));
    }

    [Fact]
    public async Task Validate_WhenCreatingSubtaskWithMatchingTicket_DoesNotQueryTicketRepository()
    {
        var command = ValidCommand();
        command.ParentWorkTaskId = Guid.NewGuid();
        WorkTasksRepositoryMock.Setup(x => x.FindParentAsync(command.ProjectId, command.ParentWorkTaskId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTask { ParentWorkTaskId = null, WorkTicketId = command.WorkTicketId });

        var result = await Validator().ValidateAsync(command);

        Assert.True(result.IsValid);
        WorkTasksRepositoryMock.Verify(
            x => x.IsWorkTicketExistAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        WorkTasksRepositoryMock.Verify(
            x => x.FindParentAsync(command.ProjectId, command.ParentWorkTaskId.Value, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Validate_WhenSubtaskTicketDiffersFromParent_UsesParentTicket()
    {
        var command = ValidCommand();
        command.ParentWorkTaskId = Guid.NewGuid();
        WorkTasksRepositoryMock.Setup(x => x.FindParentAsync(command.ProjectId, command.ParentWorkTaskId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTask { WorkTicketId = Guid.NewGuid() });

        var result = await Validator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenSubtaskTicketIsMissing_UsesParentTicket()
    {
        var command = ValidCommand();
        command.WorkTicketId = Guid.Empty;
        command.ParentWorkTaskId = Guid.NewGuid();
        WorkTasksRepositoryMock.Setup(x => x.FindParentAsync(command.ProjectId, command.ParentWorkTaskId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTask { WorkTicketId = Guid.NewGuid() });

        var result = await Validator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenAssigneeDoesNotHaveEmployeeProjectRole_FailsAssigneeValidation()
    {
        var command = ValidCommand();
        command.AssigneeId = Guid.NewGuid();
        WorkTasksRepositoryMock.Setup(x => x.IsStaffProjectParticipantExistAsync(
                command.ProjectId,
                command.AssigneeId.Value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await Validator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.AssigneeId));
    }

    private CreateWorkTaskValidator Validator() => new(WorkTasksRepositoryMock.Object, DictionariesRepositoryMock.Object);

    private static CreateWorkTaskCommand ValidCommand() => new()
    {
        ProjectId = Guid.NewGuid(),
        WorkTicketId = Guid.NewGuid(),
        Title = "Task",
        PriorityId = 1
    };
}
