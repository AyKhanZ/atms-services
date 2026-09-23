using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Validation.WorkTasks;
using Moq;

namespace Project.Services.Tests.Validators.WorkTasks;

public class MoveWorkTaskValidatorTest: BaseValidatorTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _workTaskId = Guid.NewGuid();

    public MoveWorkTaskValidatorTest()
    {
        DictionariesRepositoryMock
            .Setup(repository => repository.IsWorkTaskStatusExistAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        WorkProjectsRepositoryMock
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        WorkTasksRepositoryMock
            .Setup(repository => repository.IsWorkTaskExistAsync(
                _projectId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private MoveWorkTaskValidator Validator() =>
        new(WorkProjectsRepositoryMock.Object, WorkTasksRepositoryMock.Object, DictionariesRepositoryMock.Object);

    private void Missing(Guid workTaskId) => WorkTasksRepositoryMock
        .Setup(repository => repository.IsWorkTaskExistAsync(_projectId, workTaskId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    private MoveWorkTaskCommand Command() => new()
    {
        ProjectId = _projectId,
        WorkTaskId = _workTaskId,
        StatusId = 2
    };

    [Fact]
    public async Task Validate_AcceptsADropBetweenTwoCardsOfTheProject()
    {
        var command = Command();
        command.PreviousWorkTaskId = Guid.NewGuid();
        command.NextWorkTaskId = Guid.NewGuid();

        var result = await Validator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_RejectsATaskFromOutsideTheProject()
    {
        Missing(_workTaskId);

        var result = await Validator().ValidateAsync(Command());

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(MoveWorkTaskCommand.WorkTaskId));
    }

    [Fact]
    public async Task Validate_RejectsAProjectThatDoesNotExist()
    {
        WorkProjectsRepositoryMock
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await Validator().ValidateAsync(Command());

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(MoveWorkTaskCommand.ProjectId));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_RejectsANeighbourFromOutsideTheProject(bool above)
    {
        var neighbour = Guid.NewGuid();
        Missing(neighbour);
        var command = Command();
        command.PreviousWorkTaskId = above ? neighbour : null;
        command.NextWorkTaskId = above ? null : neighbour;

        var result = await Validator().ValidateAsync(command);

        var field = above
            ? nameof(MoveWorkTaskCommand.PreviousWorkTaskId)
            : nameof(MoveWorkTaskCommand.NextWorkTaskId);
        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Fact]
    public async Task Validate_AcceptsADropIntoAnEmptyColumn()
    {
        var result = await Validator().ValidateAsync(Command());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_RejectsMissingIdentifiers()
    {
        var result = await Validator().ValidateAsync(new MoveWorkTaskCommand());

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(MoveWorkTaskCommand.ProjectId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(MoveWorkTaskCommand.WorkTaskId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(MoveWorkTaskCommand.StatusId));
    }
}
