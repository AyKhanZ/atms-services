using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.History;
using ATMS.Project.Data.Criteria.History;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.History.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.History;

// Not cached: every save adds to it, and it is read far less often than it is written.
public class GetHistoryHandler(
    IHistoryScopeService historyScopeService,
    IHistoryRepository historyRepository,
    IHistoryValueResolver historyValueResolver)
    : IRequestHandler<GetHistoryRequest, KeysetPagedResult<HistoryEntryModel>>
{
    public async Task<KeysetPagedResult<HistoryEntryModel>> Handle(
        GetHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var scope = await historyScopeService.ResolveAsync(
            request.ProjectId,
            request.WorkTicketId,
            request.WorkTaskId,
            cancellationToken);

        ACriteria<HistoryEntry> criteria = scope.EntityType == HistoryEntityTypeEnum.Project
            ? new HistoryOfProjectCriteria(request.ProjectId)
            : new HistoryOfEntityCriteria(scope.EntityType, scope.EntityId);
        var pagination = new KeysetPaginationCriteria<HistoryEntry>(
            request.Cursor,
            request.PageSize,
            request.SortDirection);

        var page = await historyRepository.GetManyAsync(criteria, pagination, cancellationToken);
        var items = await historyValueResolver.ResolveEntriesAsync(page.Items, cancellationToken);

        return new KeysetPagedResult<HistoryEntryModel>
        {
            Items = items.ToArray(),
            NextCursor = page.NextCursor,
            HasMore = page.HasMore,
            PageSize = page.PageSize
        };
    }
}
