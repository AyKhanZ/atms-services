using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Comments.Interfaces;
using ATMS.Project.Services.Handlers.Comments;
using Moq;

namespace Project.Services.Tests.Handlers.Comments;

public sealed class UpdateCommentHandlerTest
{
    [Fact]
    public async Task Update_ChangesTextAndSavesOnce()
    {
        var projectId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var comment = new Comment { Id = commentId, Text = "Old" };
        var comments = new Mock<ICommentRepository>();
        var models = new Mock<ICommentModelService>();
        comments.Setup(repository => repository.FindAsync(
                projectId, commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        models.Setup(service => service.BuildAsync(
                projectId, It.IsAny<IReadOnlyCollection<Comment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentModel>
            {
                [commentId] = new() { Id = commentId, CreatedBy = new() }
            });

        await new UpdateCommentHandler(comments.Object, models.Object).Handle(new UpdateCommentCommand
        {
            ProjectId = projectId, CommentId = commentId, Text = "New"
        }, CancellationToken.None);

        Assert.Equal("New", comment.Text);
        comments.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MissingComment_ReturnsNotFound()
    {
        var comments = new Mock<ICommentRepository>();
        var handler = new UpdateCommentHandler(comments.Object, new Mock<ICommentModelService>().Object);

        await Assert.ThrowsAsync<EntityException>(() => handler.Handle(new UpdateCommentCommand
        {
            ProjectId = Guid.NewGuid(), CommentId = Guid.NewGuid(), Text = "New"
        }, CancellationToken.None));

        comments.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
