using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Comments.Interfaces;
using ATMS.Project.Services.Handlers.Comments;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using Moq;

namespace Project.Services.Tests.Handlers.Comments;

public sealed class CreateCommentHandlerTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<ICommentModelService> _models = new();
    private readonly Mock<ICommentNotificationService> _notifications = new();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _taskId = Guid.NewGuid();

    private CreateCommentHandler Handler() => new(_comments.Object, _models.Object, _notifications.Object);

    [Fact]
    public async Task Create_SavesComment()
    {
        var newId = Guid.NewGuid();
        Comment? saved = null;
        _comments.Setup(repository => repository.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Callback<Comment, CancellationToken>((comment, _) => { comment.Id = newId; saved = comment; })
            .Returns(Task.CompletedTask);
        _models.Setup(service => service.BuildAsync(_projectId, It.IsAny<IReadOnlyCollection<Comment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentModel> { [newId] = Model(newId) });

        await Handler().Handle(new CreateCommentCommand
        {
            ProjectId = _projectId, WorkTaskId = _taskId, Text = "Hello"
        }, CancellationToken.None);

        Assert.Equal("Hello", saved?.Text);
    }

    [Fact]
    public async Task Create_NotifiesAboutTheCommentBeforeItIsSaved()
    {
        var newId = Guid.NewGuid();
        var steps = new List<string>();
        _comments.Setup(repository => repository.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Callback<Comment, CancellationToken>((comment, _) => comment.Id = newId)
            .Returns(Task.CompletedTask);
        _notifications.Setup(service => service.NotifyCreatedAsync(
                _projectId,
                It.Is<Comment>(comment => comment.Id == newId && comment.OwnerId == _taskId),
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("notify"))
            .Returns(Task.CompletedTask);
        _comments.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("save"))
            .Returns(Task.CompletedTask);
        _models.Setup(service => service.BuildAsync(_projectId, It.IsAny<IReadOnlyCollection<Comment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentModel> { [newId] = Model(newId) });

        await Handler().Handle(new CreateCommentCommand
        {
            ProjectId = _projectId, WorkTaskId = _taskId, Text = "Hello"
        }, CancellationToken.None);

        // The notification rides on the comment's own save: no comment, no notification.
        Assert.Equal(["notify", "save"], steps);
    }

    private static CommentModel Model(Guid id) => new()
    {
        Id = id,
        CreatedBy = new PersonModel()
    };
}
