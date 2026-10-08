using ATMS.Data.Criteria;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.WorkTaskBoard;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTaskBoard;

public class GetWorkTaskBoardHandlerTest : BaseHandlerTest
{
    [Theory]
    [InlineData(WorkTaskBoardSortEnum.Title, SortDirectionEnum.Asc, "Alpha", "Beta")]
    [InlineData(WorkTaskBoardSortEnum.Title, SortDirectionEnum.Desc, "Beta", "Alpha")]
    [InlineData(WorkTaskBoardSortEnum.State, SortDirectionEnum.Asc, "Beta", "Alpha")]
    [InlineData(WorkTaskBoardSortEnum.State, SortDirectionEnum.Desc, "Alpha", "Beta")]
    [InlineData(WorkTaskBoardSortEnum.Code, SortDirectionEnum.Asc, "Beta", "Alpha")]
    [InlineData(WorkTaskBoardSortEnum.Code, SortDirectionEnum.Desc, "Alpha", "Beta")]
    public async Task Handle_SortsAndPagesWithStableCursor(
        WorkTaskBoardSortEnum sort, SortDirectionEnum direction, string first, string last)
    {
        WorkTask[] tasks = [
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Code = "9", Title = "Beta", StatusId = 1 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Code = "100", Title = "Alpha", StatusId = 3 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Code = "10", Title = "Alpha", StatusId = 3 }
        ];
        var repository = new Mock<IWorkTaskBoardRepository>();
        MapperMock.Setup(mapper => mapper.Map<WorkTaskBoardFilter>(It.IsAny<GetWorkTaskBoardRequest>()))
            .Returns(new WorkTaskBoardFilter());
        MapperMock.Setup(mapper => mapper.Map<WorkTaskModel>(It.IsAny<WorkTask>()))
            .Returns<WorkTask>(task => new WorkTaskModel { Id = task.Id, Code = task.Code, Title = task.Title });
        repository.Setup(repo => repo.GetManyAsync(
                It.IsAny<ICriteria<WorkTask>>(), It.IsAny<IKeysetPagination<WorkTask>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Returns<ICriteria<WorkTask>, IKeysetPagination<WorkTask>, IReadOnlyCollection<string>, CancellationToken>((_, pagination, _, _) =>
            {
                var page = pagination.ToResult(pagination.Apply(tasks.AsQueryable()).ToArray());
                return Task.FromResult(new WorkTasksQueryResult(page, new Dictionary<Guid, WorkTaskProgress>()));
            });
        var handler = new GetWorkTaskBoardHandler(CurrentUserMock.Object, repository.Object, MapperMock.Object);
        var results = new List<WorkTaskModel>();
        string? cursor = null;
        do
        {
            var result = await handler.Handle(new GetWorkTaskBoardRequest
            {
                Sort = (int)sort, SortDirection = direction, PageSize = 1, Cursor = cursor
            }, CancellationToken.None);
            results.AddRange(result.Items);
            cursor = result.NextCursor;
        } while (cursor is not null);

        Assert.Equal(3, results.Count);
        Assert.Equal(3, results.Select(task => task.Id).Distinct().Count());
        Assert.Equal(first, results[0].Title);
        Assert.Equal(last, results[^1].Title);
        if (sort == WorkTaskBoardSortEnum.Code)
        {
            string[] expected = direction == SortDirectionEnum.Asc ? ["9", "10", "100"] : ["100", "10", "9"];
            Assert.Equal(expected, results.Select(task => task.Code));
        }
    }

    [Theory]
    [InlineData(WorkTaskBoardSortEnum.Title)]
    [InlineData(WorkTaskBoardSortEnum.State)]
    [InlineData(WorkTaskBoardSortEnum.Code)]
    public async Task Handle_NewSortCursorTranslatesToPostgreSql(WorkTaskBoardSortEnum sort)
    {
        var repository = new Mock<IWorkTaskBoardRepository>();
        MapperMock.Setup(mapper => mapper.Map<WorkTaskBoardFilter>(It.IsAny<GetWorkTaskBoardRequest>()))
            .Returns(new WorkTaskBoardFilter());
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_compilation_only").Options;
        await using var context = new ProjectDbContext(options);
        string? sql = null;
        repository.Setup(repo => repo.GetManyAsync(
                It.IsAny<ICriteria<WorkTask>>(), It.IsAny<IKeysetPagination<WorkTask>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Returns<ICriteria<WorkTask>, IKeysetPagination<WorkTask>, IReadOnlyCollection<string>, CancellationToken>((_, pagination, _, _) =>
            {
                sql = pagination.Apply(context.WorkTasks).ToQueryString();
                return Task.FromResult(new WorkTasksQueryResult(
                    new KeysetPagedResult<WorkTask> { Items = [], PageSize = 20 },
                    new Dictionary<Guid, WorkTaskProgress>()));
            });
        var cursor = sort switch
        {
            WorkTaskBoardSortEnum.Title => KeysetCursor.For("Alpha", Guid.NewGuid(), SortDirectionEnum.Asc, sort.ToString()).Encode(),
            WorkTaskBoardSortEnum.Code => KeysetCursor.For("10".PadLeft(50, '0'), Guid.NewGuid(), SortDirectionEnum.Asc, sort.ToString()).Encode(),
            _ => KeysetCursor.For(1, Guid.NewGuid(), SortDirectionEnum.Asc, sort.ToString()).Encode()
        };

        await new GetWorkTaskBoardHandler(CurrentUserMock.Object, repository.Object, MapperMock.Object)
            .Handle(new GetWorkTaskBoardRequest { Sort = (int)sort, Cursor = cursor, SortDirection = SortDirectionEnum.Asc }, CancellationToken.None);

        Assert.Contains("ORDER BY", sql);
        Assert.Contains(sort switch
        {
            WorkTaskBoardSortEnum.Title => "\"Title\"",
            WorkTaskBoardSortEnum.Code => "lpad",
            _ => "\"StatusId\""
        }, sql);
    }
}
