using ATMS.Data.Constants;
using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Search;
using ATMS.Project.Contracts.Requests.Search;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Search;

public sealed class GetGlobalSearchPageHandler(
    ICurrentUser currentUser,
    IGlobalSearchRepository repository,
    IMapper mapper) : IRequestHandler<GetGlobalSearchPageRequest, KeysetPagedResult<GlobalSearchItemModel>>
{
    public async Task<KeysetPagedResult<GlobalSearchItemModel>> Handle(
        GetGlobalSearchPageRequest request,
        CancellationToken cancellationToken)
    {
        // query, page size, sort and cursor are already checked by ValidationBehavior
        KeysetCursor.TryDecode(request.Cursor, out var cursor);

        var rows = await repository.SearchPageAsync(
            currentUser.Id,
            currentUser.RoleId == RoleIds.SuperAdmin,
            request.Q!,
            (GlobalSearchItemTypeEnum)request.ItemType,
            cursor,
            request.SortDirection,
            request.PageSize,
            CultureHelper.CurrentLanguage,
            cancellationToken);

        // one extra row = there is a next page, it's not sent to the client
        var hasMore = rows.Length > request.PageSize;
        var page = rows.Take(request.PageSize).ToArray();
        var last = page.LastOrDefault();

        return new KeysetPagedResult<GlobalSearchItemModel>
        {
            Items = mapper.Map<GlobalSearchItemModel[]>(page),
            PageSize = request.PageSize,
            HasMore = hasMore,
            NextCursor = hasMore && last is not null
                ? KeysetCursor.For(last.CreatedAt, last.Id, request.SortDirection).Encode()
                : null,
        };
    }
}
