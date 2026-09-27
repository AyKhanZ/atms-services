using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Comments.Interfaces;
using ATMS.Project.Services.Handlers.Comments;
using Moq;

namespace Project.Services.Tests.Handlers.Comments;

public sealed class CreateCommentHandlerTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<ICommentModelService> _models = new();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _taskId = Guid.NewGuid();

    private CreateCommentHandler Handler() => new(_comments.Object, _models.Object);

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

    private static CommentModel Model(Guid id) => new()
    {
        Id = id,
        CreatedBy = new HistoryPersonModel()
    };
}
