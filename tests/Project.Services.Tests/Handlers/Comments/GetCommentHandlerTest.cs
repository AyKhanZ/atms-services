using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Comments.Interfaces;
using ATMS.Project.Services.Handlers.Comments;
using Moq;

namespace Project.Services.Tests.Handlers.Comments;

public sealed class GetCommentHandlerTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<ICommentModelService> _models = new();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _commentId = Guid.NewGuid();

    private GetCommentHandler Handler() => new(_comments.Object, _models.Object);

    private GetCommentRequest Request() => new() { ProjectId = _projectId, CommentId = _commentId };

    [Fact]
    public async Task LiveComment_ReturnsItsModel()
    {
        var model = new CommentModel { Id = _commentId, CreatedBy = new PersonModel() };
        _comments.Setup(value => value.GetAsync(_projectId, _commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment { Id = _commentId, Text = "Hi" });
        _models.Setup(value => value.BuildAsync(
                _projectId, It.IsAny<IReadOnlyCollection<Comment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentModel> { [_commentId] = model });

        var result = await Handler().Handle(Request(), CancellationToken.None);

        Assert.Same(model, result);
    }

    // A deleted comment is not found by the repository itself, which the repository test covers.
    [Fact]
    public async Task MissingComment_ReturnsNotFound()
    {
        await Assert.ThrowsAsync<EntityException>(() => Handler().Handle(Request(), CancellationToken.None));

        _models.VerifyNoOtherCalls();
    }
}
