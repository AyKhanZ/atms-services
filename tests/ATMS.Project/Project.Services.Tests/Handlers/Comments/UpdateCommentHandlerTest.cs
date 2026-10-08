using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Comments.Interfaces;
using ATMS.Project.Services.Handlers.Comments;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using Moq;

namespace Project.Services.Tests.Handlers.Comments;

public sealed class UpdateCommentHandlerTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<ICommentModelService> _models = new();
    private readonly Mock<ICommentNotificationService> _notifications = new();

    private UpdateCommentHandler Handler() => new(_comments.Object, _models.Object, _notifications.Object);

    [Fact]
    public async Task Update_ChangesTextAndSavesOnce()
    {
        var projectId = Guid.NewGuid();
        var comment = SetupComment(projectId, "Old");

        await Handler().Handle(new UpdateCommentCommand
        {
            ProjectId = projectId, CommentId = comment.Id, Text = "New"
        }, CancellationToken.None);

        Assert.Equal("New", comment.Text);
        _comments.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_PassesThePreviousTextSoOnlyNewMentionsAreNotified()
    {
        var projectId = Guid.NewGuid();
        var comment = SetupComment(projectId, "Old");
        var steps = new List<string>();
        _notifications.Setup(service => service.NotifyEditedAsync(
                projectId,
                It.Is<Comment>(edited => edited.Id == comment.Id && edited.Text == "New"),
                "Old",
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("notify"))
            .Returns(Task.CompletedTask);
        _comments.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("save"))
            .Returns(Task.CompletedTask);

        await Handler().Handle(new UpdateCommentCommand
        {
            ProjectId = projectId, CommentId = comment.Id, Text = "New"
        }, CancellationToken.None);

        Assert.Equal(["notify", "save"], steps);
    }

    [Fact]
    public async Task MissingComment_ReturnsNotFound()
    {
        await Assert.ThrowsAsync<EntityException>(() => Handler().Handle(new UpdateCommentCommand
        {
            ProjectId = Guid.NewGuid(), CommentId = Guid.NewGuid(), Text = "New"
        }, CancellationToken.None));

        _comments.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notifications.VerifyNoOtherCalls();
    }

    private Comment SetupComment(Guid projectId, string text)
    {
        var comment = new Comment { Id = Guid.NewGuid(), Text = text };
        _comments.Setup(repository => repository.FindAsync(projectId, comment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        _models.Setup(service => service.BuildAsync(
                projectId, It.IsAny<IReadOnlyCollection<Comment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentModel>
            {
                [comment.Id] = new() { Id = comment.Id, CreatedBy = new() }
            });
        return comment;
    }
}
