using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.History;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.History.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.History;

public sealed class GetHistoryStatesHandler(
    IHistoryScopeService historyScopeService,
    IHistoryRepository historyRepository,
    IHistoryValueResolver historyValueResolver)
    : IRequestHandler<GetHistoryStatesRequest, IReadOnlyCollection<HistoryStateModel>>
{
    // 200 is way more than anyone scrolls back
    private const int MaxStates = 200;

    public async Task<IReadOnlyCollection<HistoryStateModel>> Handle(
        GetHistoryStatesRequest request,
        CancellationToken cancellationToken)
    {
        var scope = await historyScopeService.ResolveAsync(
            request.ProjectId,
            request.WorkTicketId,
            request.WorkTaskId,
            cancellationToken);

        // take one more to know if the list was cut
        var latest = await historyRepository.GetStatusChangesAsync(
            scope.EntityType,
            scope.EntityId,
            MaxStates + 1,
            cancellationToken);
        var cut = latest.Length > MaxStates;
        var changes = cut ? latest[1..] : latest;

        var creation = cut
            ? null
            : await historyRepository.GetCreationAsync(scope.EntityType, scope.EntityId, cancellationToken);

        return await historyValueResolver.ResolveStatesAsync(
            scope.EntityType,
            changes,
            creation,
            cancellationToken);
    }
}
