using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.History;
using ATMS.Project.Data.Models.History;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.History;
using ATMS.Project.Services.History.Interfaces;
using ATMS.Project.Services.Models.History;
using Moq;

namespace Project.Services.Tests.Handlers.History;

public class GetHistoryStatesHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _taskId = Guid.NewGuid();
    private readonly Mock<IHistoryScopeService> _scopeServiceMock = new();
    private readonly Mock<IHistoryRepository> _historyRepositoryMock = new();
    private readonly Mock<IHistoryValueResolver> _valueResolverMock = new();
    private readonly HistoryStatusChange _creation = new(DateTime.UtcNow.AddDays(-30), Guid.NewGuid(), null, null);

    public GetHistoryStatesHandlerTest()
    {
        _scopeServiceMock
            .Setup(service => service.ResolveAsync(_projectId, null, _taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HistoryScope(HistoryEntityTypeEnum.WorkTask, _taskId));
        _historyRepositoryMock
            .Setup(repository => repository.GetCreationAsync(
                HistoryEntityTypeEnum.WorkTask,
                _taskId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_creation);
        _valueResolverMock
            .Setup(resolver => resolver.ResolveStatesAsync(
                It.IsAny<HistoryEntityTypeEnum>(),
                It.IsAny<IReadOnlyCollection<HistoryStatusChange>>(),
                It.IsAny<HistoryStatusChange?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    private GetHistoryStatesHandler Handler() =>
        new(_scopeServiceMock.Object, _historyRepositoryMock.Object, _valueResolverMock.Object);

    private void Changes(int count)
    {
        _historyRepositoryMock
            .Setup(repository => repository.GetStatusChangesAsync(
                HistoryEntityTypeEnum.WorkTask,
                _taskId,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Range(0, count)
                .Select(index => new HistoryStatusChange(DateTime.UtcNow.AddMinutes(index), null, "1", "2"))
                .ToArray());
    }

    [Fact]
    public async Task Handle_EveryStatusSinceTheCreation_DatesTheFirstOneByTheCreation()
    {
        Changes(3);

        await Handler().Handle(new GetHistoryStatesRequest { ProjectId = _projectId, WorkTaskId = _taskId }, CancellationToken.None);

        _valueResolverMock.Verify(resolver => resolver.ResolveStatesAsync(
            HistoryEntityTypeEnum.WorkTask,
            It.IsAny<IReadOnlyCollection<HistoryStatusChange>>(),
            _creation,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Handle_ListCutAtTheLimit_LeavesTheCreationOut()
    {
        Changes(200);

        await Handler().Handle(new GetHistoryStatesRequest { ProjectId = _projectId, WorkTaskId = _taskId }, CancellationToken.None);

        _valueResolverMock.Verify(resolver => resolver.ResolveStatesAsync(
            HistoryEntityTypeEnum.WorkTask,
            It.IsAny<IReadOnlyCollection<HistoryStatusChange>>(),
            null,
            It.IsAny<CancellationToken>()));
        _historyRepositoryMock.Verify(
            repository => repository.GetCreationAsync(
                It.IsAny<HistoryEntityTypeEnum>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
