using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.History;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.History.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.History;

public class GetHistoryStatesHandler(
    IHistoryScopeService historyScopeService,
    IHistoryRepository historyRepository,
    IHistoryValueResolver historyValueResolver)
    : IRequestHandler<GetHistoryStatesRequest, IReadOnlyCollection<HistoryStateModel>>
{
    // The status bar shows the latest six and folds the rest; 200 is far past what anyone unfolds.
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

        var changes = await historyRepository.GetStatusChangesAsync(
            scope.EntityType,
            scope.EntityId,
            MaxStates,
            cancellationToken);

        // Every status since the creation is here: the one the item started with was set when it was
        // created. A list cut at the limit starts somewhere later, and that start has no date.
        var creation = changes.Length < MaxStates
            ? await historyRepository.GetCreationAsync(scope.EntityType, scope.EntityId, cancellationToken)
            : null;

        return await historyValueResolver.ResolveStatesAsync(
            scope.EntityType,
            changes,
            creation,
            cancellationToken);
    }
}
