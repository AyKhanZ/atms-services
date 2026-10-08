using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.Comments;
using Moq;

namespace Project.Services.Tests.Validators.Comments;

public sealed class DeleteCommentValidatorTest : BaseValidatorTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _commentId = Guid.NewGuid();

    public DeleteCommentValidatorTest()
    {
        WorkProjectsRepositoryMock.Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _comments.Setup(repository => repository.IsLiveCommentAsync(
                _projectId, _commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private DeleteCommentValidator Validator() => new(WorkProjectsRepositoryMock.Object, _comments.Object);

    [Fact]
    public async Task LiveComment_IsValid()
    {
        var result = await Validator().ValidateAsync(new DeleteCommentCommand
        {
            ProjectId = _projectId, CommentId = _commentId
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CommentOutsideProject_IsRejected()
    {
        var result = await Validator().ValidateAsync(new DeleteCommentCommand
        {
            ProjectId = _projectId, CommentId = Guid.NewGuid()
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(DeleteCommentCommand.CommentId) &&
            error.ErrorMessage == "This comment was deleted or is no longer available. Refresh the page.");
    }

    [Fact]
    public async Task EmptyCommentId_UsesRequiredMessage()
    {
        var result = await Validator().ValidateAsync(new DeleteCommentCommand
        {
            ProjectId = _projectId, CommentId = Guid.Empty
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(DeleteCommentCommand.CommentId) &&
            error.ErrorMessage == "Choose a comment.");
    }
}
