using System.Linq.Expressions;
using ATMS.Application.Interfaces;
using ATMS.Data.Constants;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.Comments;
using Moq;

namespace Project.Services.Tests.Validators.Comments;

public sealed class UpdateCommentValidatorTest : BaseValidatorTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _commentId = Guid.NewGuid();
    private readonly Guid _authorId = Guid.NewGuid();

    public UpdateCommentValidatorTest()
    {
        WorkProjectsRepositoryMock.Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _comments.Setup(repository => repository.GetAuthorIdAsync(
                _projectId, _commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_authorId);
        _currentUser.SetupGet(user => user.Id).Returns(_authorId);
    }

    private UpdateCommentValidator Validator() => new(
        WorkProjectsRepositoryMock.Object, _comments.Object, _currentUser.Object);

    [Theory]
    [InlineData(" ", false)]
    [InlineData("Edited", true)]
    public async Task Text_MustNotBeWhitespace(string text, bool valid)
    {
        var result = await Validator().ValidateAsync(new UpdateCommentCommand
        {
            ProjectId = _projectId, CommentId = _commentId, Text = text
        });

        Assert.Equal(valid, result.IsValid);
    }

    [Fact]
    public async Task MissingComment_IsRejected()
    {
        var result = await Validator().ValidateAsync(new UpdateCommentCommand
        {
            ProjectId = _projectId, CommentId = Guid.NewGuid(), Text = "Edited"
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(UpdateCommentCommand.CommentId) &&
            error.ErrorMessage == "This comment was deleted or is no longer available. Refresh the page.");
        _comments.Verify(repository => repository.IsLiveCommentAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EmptyCommentId_UsesRequiredMessage()
    {
        var result = await Validator().ValidateAsync(new UpdateCommentCommand
        {
            ProjectId = _projectId, CommentId = Guid.Empty, Text = "Edited"
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(UpdateCommentCommand.CommentId) &&
            error.ErrorMessage == "Choose a comment.");
    }

    [Fact]
    public async Task OtherAuthorsComment_IsRejected()
    {
        _currentUser.SetupGet(user => user.Id).Returns(Guid.NewGuid());

        var result = await Validator().ValidateAsync(new UpdateCommentCommand
        {
            ProjectId = _projectId, CommentId = _commentId, Text = "Edited"
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(UpdateCommentCommand.CommentId) &&
            error.ErrorMessage == "You can edit only your own comments.");
    }

    [Fact]
    public async Task SuperAdmin_CannotEditOtherAuthorsComment()
    {
        _currentUser.SetupGet(user => user.Id).Returns(Guid.NewGuid());
        _currentUser.SetupGet(user => user.RoleId).Returns(RoleIds.SuperAdmin);

        var result = await Validator().ValidateAsync(new UpdateCommentCommand
        {
            ProjectId = _projectId, CommentId = _commentId, Text = "Changed"
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(UpdateCommentCommand.CommentId) &&
            error.ErrorMessage == "You can edit only your own comments.");
    }
}
