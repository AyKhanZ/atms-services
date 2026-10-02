using ATMS.Application.Interfaces;
using ATMS.Application.Models;
using ATMS.Data.Constants;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Comments;
using ATMS.Project.Services.Dictionaries.Interfaces;
using ATMS.Project.Services.Security.Interfaces;
using Moq;

namespace Project.Services.Tests.Comments;

public sealed class CommentModelServiceTest
{
    private readonly Mock<IDictionaryCacheService> _dictionaries = new();

    public CommentModelServiceTest()
    {
        _dictionaries.Setup(value => value.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DictionaryModel { Id = 2, Code = "InProgress", Name = "In progress" }]);
        _dictionaries.Setup(value => value.GetWorkTicketStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DictionaryModel { Id = 3, Code = "Review", Name = "Review" }]);
        _dictionaries.Setup(value => value.GetProjectStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DictionaryModel { Id = 1, Code = "Active", Name = "Active" }]);
    }

    [Fact]
    public async Task Build_ResolvesOnlyProjectParticipantsAndUsesCurrentNames()
    {
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var knownId = Guid.NewGuid();
        var unknownId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            CreatedById = authorId,
            Text = $"Hello @[user:{knownId}] and @[user:{unknownId}]",
        };
        var repository = new Mock<ICommentRepository>();
        var permissions = new Mock<IProjectPermissionService>();
        var user = new Mock<ICurrentUser>();
        user.SetupGet(value => value.Id).Returns(authorId);
        permissions.Setup(value => value.GetPermissionCodesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "CommentEdit" });
        repository.Setup(value => value.GetAuthorsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new User { Id = authorId, Name = "Author", Surname = "New", AvatarPath = "avatar-a" }]);
        repository.Setup(value => value.GetMentionedParticipantsAsync(projectId,
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new User { Id = knownId, Name = "Current", Surname = "Name", AvatarPath = "avatar-b" }]);

        var service = new CommentModelService(repository.Object, permissions.Object, _dictionaries.Object, user.Object);
        var models = await service.BuildAsync(projectId, [comment], CancellationToken.None);

        var model = models[comment.Id];
        Assert.Equal(comment.Text, model.Text);
        Assert.True(model.CanEdit);
        Assert.True(model.CanDelete);
        Assert.Equal("Author", model.CreatedBy.Name);
        var mention = Assert.Single(model.Mentions);
        Assert.Equal(knownId, mention.Id);
        Assert.Equal("Current", mention.Name);
        Assert.Equal("Name", mention.Surname);
        Assert.Equal("avatar-b", mention.AvatarPath);
    }

    [Fact]
    public async Task Build_ResolvesReferencesOutsideCodeAndMarkdownLinks()
    {
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = Guid.NewGuid(), CreatedById = authorId,
            Text = "See #41 and `#42` and [#43](https://example.com/#44), then #45 in #180."
        };
        var repository = new Mock<ICommentRepository>();
        var permissions = new Mock<IProjectPermissionService>();
        var user = new Mock<ICurrentUser>();
        user.SetupGet(value => value.Id).Returns(authorId);
        user.SetupGet(value => value.RoleId).Returns(RoleIds.Employee);
        permissions.Setup(value => value.GetPermissionCodesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());
        repository.Setup(value => value.GetAuthorsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new User { Id = authorId, Name = "Author", Surname = "Name" }]);
        repository.Setup(value => value.GetReferencesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<ICriteria<WorkProject>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new CommentWorkItemReferenceRow("41", CommentReferenceKind.Task, true, "Task title", 2,
                    projectId, ticketId, taskId),
                new CommentWorkItemReferenceRow("45", CommentReferenceKind.Ticket, false, "Ticket title", 3,
                    projectId, ticketId, null),
                new CommentWorkItemReferenceRow("180", CommentReferenceKind.Project, false, "Resort", 1,
                    projectId, null, null)
            ]);

        var model = (await new CommentModelService(repository.Object, permissions.Object, _dictionaries.Object, user.Object)
            .BuildAsync(projectId, [comment], CancellationToken.None))[comment.Id];

        Assert.Equal(["41", "45", "180"], model.References.Select(reference => reference.Code));
        Assert.Equal("task", model.References[0].Type);
        Assert.True(model.References[0].IsSubtask);
        Assert.Equal("Task title", model.References[0].Title);
        Assert.Equal("InProgress", model.References[0].Status.Code);
        Assert.Equal("In progress", model.References[0].Status.Name);
        Assert.Equal(taskId, model.References[0].Ref.WorkTaskId);
        Assert.Equal("ticket", model.References[1].Type);
        Assert.Equal(ticketId, model.References[1].Ref.WorkTicketId);
        Assert.Equal("Review", model.References[1].Status.Name);
        Assert.Equal("project", model.References[2].Type);
        Assert.Equal("Active", model.References[2].Status.Name);
        Assert.Null(model.References[2].Ref.WorkTicketId);
        repository.Verify(value => value.GetReferencesAsync(
            It.Is<IReadOnlyCollection<string>>(codes => codes.SequenceEqual(new[] { "41", "45", "180" })),
            It.IsAny<ICriteria<WorkProject>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Build_OddRows_SkipsThemInsteadOfFailing()
    {
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var comment = new Comment { Id = Guid.NewGuid(), CreatedById = authorId, Text = "See #41, #42 and #43" };
        var repository = new Mock<ICommentRepository>();
        repository.Setup(value => value.GetAuthorsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        repository.Setup(value => value.GetReferencesAsync(
                It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<ICriteria<WorkProject>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new CommentWorkItemReferenceRow("41", CommentReferenceKind.Task, false, "First", 2,
                    projectId, null, Guid.NewGuid()),
                new CommentWorkItemReferenceRow("41", CommentReferenceKind.Ticket, false, "Second", 3,
                    projectId, Guid.NewGuid(), null),
                new CommentWorkItemReferenceRow("42", CommentReferenceKind.Task, false, "Unknown status", 99,
                    projectId, null, Guid.NewGuid())
            ]);
        var user = new Mock<ICurrentUser>();
        user.SetupGet(value => value.Id).Returns(Guid.NewGuid());
        var permissions = new Mock<IProjectPermissionService>();
        permissions.Setup(value => value.GetPermissionCodesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        var model = (await new CommentModelService(repository.Object, permissions.Object, _dictionaries.Object, user.Object)
            .BuildAsync(projectId, [comment], CancellationToken.None))[comment.Id];

        Assert.Equal(authorId, model.CreatedBy.Id);
        Assert.Equal(string.Empty, model.CreatedBy.Name);
        var reference = Assert.Single(model.References);
        Assert.Equal("First", reference.Title);
    }

    [Fact]
    public async Task Build_TwentyComments_RequestReferencesOnce()
    {
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var items = Enumerable.Range(1, 20)
            .Select(number => new Comment
            {
                Id = Guid.NewGuid(), CreatedById = authorId, Text = $"See #{number}"
            })
            .ToArray();
        var repository = new Mock<ICommentRepository>();
        repository.Setup(value => value.GetAuthorsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new User { Id = authorId, Name = "Author", Surname = "Name" }]);
        repository.Setup(value => value.GetReferencesAsync(
                It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<ICriteria<WorkProject>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var user = new Mock<ICurrentUser>();
        user.SetupGet(value => value.Id).Returns(authorId);

        var permissions = new Mock<IProjectPermissionService>();
        permissions.Setup(value => value.GetPermissionCodesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        await new CommentModelService(repository.Object, permissions.Object, _dictionaries.Object, user.Object)
            .BuildAsync(projectId, items, CancellationToken.None);

        repository.Verify(value => value.GetReferencesAsync(
            It.Is<IReadOnlyCollection<string>>(codes => codes.Count == 20),
            It.IsAny<ICriteria<WorkProject>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
