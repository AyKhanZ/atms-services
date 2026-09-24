using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.History;
using ATMS.Project.Data.Criteria.History;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.History;
using ATMS.Project.Services.History.Interfaces;
using ATMS.Project.Services.Models.History;
using Moq;

namespace Project.Services.Tests.Handlers.History;

public class GetHistoryHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Mock<IHistoryScopeService> _scopeServiceMock = new();
    private readonly Mock<IHistoryRepository> _historyRepositoryMock = new();
    private readonly Mock<IHistoryValueResolver> _valueResolverMock = new();
    private ACriteria<HistoryEntry>? _criteria;

    public GetHistoryHandlerTest()
    {
        _historyRepositoryMock
            .Setup(repository => repository.GetManyAsync(
                It.IsAny<ACriteria<HistoryEntry>>(),
                It.IsAny<KeysetPaginationCriteria<HistoryEntry>>(),
                It.IsAny<CancellationToken>()))
            .Callback<ACriteria<HistoryEntry>, KeysetPaginationCriteria<HistoryEntry>, CancellationToken>(
                (criteria, _, _) => _criteria = criteria)
            .ReturnsAsync(new KeysetPagedResult<HistoryEntry>
            {
                Items = [new HistoryEntry { Id = Guid.NewGuid() }],
                HasMore = true,
                NextCursor = "next",
                PageSize = 20
            });
        _valueResolverMock
            .Setup(resolver => resolver.ResolveEntriesAsync(
                It.IsAny<IReadOnlyCollection<HistoryEntry>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HistoryEntryModel()]);
    }

    private GetHistoryHandler Handler() =>
        new(_scopeServiceMock.Object, _historyRepositoryMock.Object, _valueResolverMock.Object);

    private void Scope(HistoryEntityTypeEnum entityType, Guid entityId)
    {
        _scopeServiceMock
            .Setup(service => service.ResolveAsync(
                _projectId,
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HistoryScope(entityType, entityId));
    }

    [Fact]
    public async Task Handle_Project_ReadsTheProjectWithItsGroupsAndKeepsThePaging()
    {
        Scope(HistoryEntityTypeEnum.Project, _projectId);

        var result = await Handler().Handle(new GetHistoryRequest { ProjectId = _projectId }, CancellationToken.None);

        Assert.IsType<HistoryOfProjectCriteria>(_criteria);
        Assert.Single(result.Items);
        Assert.True(result.HasMore);
        Assert.Equal("next", result.NextCursor);
    }

    [Fact]
    public async Task Handle_Task_ReadsOnlyThatTask()
    {
        var taskId = Guid.NewGuid();
        Scope(HistoryEntityTypeEnum.WorkTask, taskId);

        await Handler().Handle(new GetHistoryRequest { ProjectId = _projectId, WorkTaskId = taskId }, CancellationToken.None);

        Assert.IsType<HistoryOfEntityCriteria>(_criteria);
    }
}
