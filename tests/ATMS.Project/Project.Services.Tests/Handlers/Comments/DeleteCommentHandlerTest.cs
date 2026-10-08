using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Comments;
using Moq;

namespace Project.Services.Tests.Handlers.Comments;

public sealed class DeleteCommentHandlerTest
{
    [Fact]
    public async Task Delete_SoftDeletesAndRecordsActor()
    {
        var projectId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var comment = new Comment { Id = commentId, Text = "Text" };
        var comments = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(user => user.Id).Returns(userId);
        comments.Setup(repository => repository.FindAsync(
                projectId, commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        await new DeleteCommentHandler(comments.Object, currentUser.Object).Handle(new DeleteCommentCommand
        {
            ProjectId = projectId, CommentId = commentId
        }, CancellationToken.None);

        Assert.True(comment.IsDeleted);
        Assert.Equal(userId, comment.DeletedById);
        comments.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MissingComment_ReturnsNotFound()
    {
        var comments = new Mock<ICommentRepository>();
        var handler = new DeleteCommentHandler(comments.Object, new Mock<ICurrentUser>().Object);

        await Assert.ThrowsAsync<EntityException>(() => handler.Handle(new DeleteCommentCommand
        {
            ProjectId = Guid.NewGuid(), CommentId = Guid.NewGuid()
        }, CancellationToken.None));

        comments.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
