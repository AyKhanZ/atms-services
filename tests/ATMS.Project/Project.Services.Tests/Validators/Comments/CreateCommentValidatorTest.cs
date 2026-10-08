using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Data.Criteria.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.Comments;
using Moq;

namespace Project.Services.Tests.Validators.Comments;

public sealed class CreateCommentValidatorTest : BaseValidatorTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _taskId = Guid.NewGuid();

    public CreateCommentValidatorTest()
    {
        WorkProjectsRepositoryMock.Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _comments.Setup(repository => repository.IsOwnerTaskLiveAsync(
                _projectId, _taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private CreateCommentValidator Validator() => new(
        WorkProjectsRepositoryMock.Object, _comments.Object);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyText_IsRejected(string text)
    {
        var result = await Validator().ValidateAsync(new CreateCommentCommand
        {
            ProjectId = _projectId, WorkTaskId = _taskId, Text = text
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCommentCommand.Text));
    }

    [Theory]
    [InlineData(2000, true)]
    [InlineData(2001, false)]
    public async Task TextLength_HasThe2000CharacterBoundary(int length, bool valid)
    {
        var result = await Validator().ValidateAsync(new CreateCommentCommand
        {
            ProjectId = _projectId, WorkTaskId = _taskId, Text = new string('x', length)
        });

        Assert.Equal(valid, result.IsValid);
    }

    [Fact]
    public async Task TaskOutsideProject_IsRejected()
    {
        var result = await Validator().ValidateAsync(new CreateCommentCommand
        {
            ProjectId = _projectId, WorkTaskId = Guid.NewGuid(), Text = "Hello"
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCommentCommand.WorkTaskId));
    }
}
