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
    // The status graph opens on the latest ones; 200 is far past what anyone scrolls back to.
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

        // One more than is shown, to tell a list of exactly 200 from one cut at 200.
        var latest = await historyRepository.GetStatusChangesAsync(
            scope.EntityType,
            scope.EntityId,
            MaxStates + 1,
            cancellationToken);
        var cut = latest.Length > MaxStates;
        var changes = cut ? latest[1..] : latest;

        // Every status since the creation is here: the one the item started with was set when it was
        // created. A cut list starts somewhere later, and that start has no date.
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
