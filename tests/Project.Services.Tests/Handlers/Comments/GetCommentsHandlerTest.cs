using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Comments;
using ATMS.Project.Data.Criteria.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Comments.Interfaces;
using ATMS.Project.Services.Handlers.Comments;
using AutoMapper;
using Moq;

namespace Project.Services.Tests.Handlers.Comments;

public sealed class GetCommentsHandlerTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<ICommentModelService> _models = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _taskId = Guid.NewGuid();

    private GetCommentsHandler Handler() => new(_comments.Object, _models.Object, _mapper.Object);

    [Fact]
    public async Task Page_KeepsTheOrderAndTheCursor()
    {
        var newer = new Comment { Id = Guid.NewGuid(), OwnerId = _taskId, Text = "Newer" };
        var older = new Comment { Id = Guid.NewGuid(), OwnerId = _taskId, Text = "Older" };
        var request = new GetCommentsRequest { ProjectId = _projectId, WorkTaskId = _taskId };
        _mapper.Setup(value => value.Map<CommentFilter>(request))
            .Returns(new CommentFilter { WorkTaskId = _taskId });
        _comments.Setup(value => value.GetManyAsync(
                _projectId, It.IsAny<ACriteria<Comment>>(), It.IsAny<KeysetPaginationCriteria<Comment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeysetPagedResult<Comment>
            {
                Items = [newer, older],
                NextCursor = "next",
                HasMore = true,
                PageSize = 20
            });
        _models.Setup(value => value.BuildAsync(
                _projectId, It.IsAny<IReadOnlyCollection<Comment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentModel>
            {
                [older.Id] = Model(older.Id),
                [newer.Id] = Model(newer.Id)
            });

        var result = await Handler().Handle(request, CancellationToken.None);

        Assert.Equal([newer.Id, older.Id], result.Items.Select(item => item.Id));
        Assert.Equal("next", result.NextCursor);
        Assert.True(result.HasMore);
    }

    [Fact]
    public async Task AscendingRequest_IsStillReadNewestFirst()
    {
        var request = new GetCommentsRequest
        {
            ProjectId = _projectId, WorkTaskId = _taskId, SortDirection = SortDirectionEnum.Asc
        };
        KeysetPaginationCriteria<Comment>? usedPagination = null;
        _mapper.Setup(value => value.Map<CommentFilter>(request))
            .Returns(new CommentFilter { WorkTaskId = _taskId });
        _comments.Setup(value => value.GetManyAsync(
                _projectId, It.IsAny<ACriteria<Comment>>(), It.IsAny<KeysetPaginationCriteria<Comment>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, ACriteria<Comment>, KeysetPaginationCriteria<Comment>, CancellationToken>(
                (_, _, pagination, _) => usedPagination = pagination)
            .ReturnsAsync(new KeysetPagedResult<Comment> { Items = [], PageSize = 20 });
        _models.Setup(value => value.BuildAsync(
                _projectId, It.IsAny<IReadOnlyCollection<Comment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentModel>());

        var result = await Handler().Handle(request, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(SortDirectionEnum.Desc, usedPagination?.SortDirection);
    }

    private static CommentModel Model(Guid id) => new() { Id = id, CreatedBy = new PersonModel() };
}
