using ATMS.Data.Constants;
using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Search;
using ATMS.Project.Contracts.Requests.Search;
using ATMS.Project.Data.Models.Search;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Search;

public class GetGlobalSearchHandler(
    ICurrentUser currentUser,
    IGlobalSearchRepository repository,
    IGlobalSearchRecentRepository recentRepository,
    IMapper mapper) : IRequestHandler<GetGlobalSearchRequest, GlobalSearchModel>
{
    public async Task<GlobalSearchModel> Handle(GetGlobalSearchRequest request, CancellationToken cancellationToken)
    {
        var search = request.Q?.Trim();
        var isSuperAdmin = currentUser.RoleId == RoleIds.SuperAdmin;
        if (string.IsNullOrEmpty(search))
        {
            var recent = await recentRepository.GetRecentAsync(
                currentUser.Id, isSuperAdmin, CultureHelper.CurrentLanguage, cancellationToken);
            return new GlobalSearchModel
            {
                Projects = new GlobalSearchGroupModel(),
                Tickets = new GlobalSearchGroupModel(),
                Tasks = new GlobalSearchGroupModel(),
                Subtasks = new GlobalSearchGroupModel(),
                Recent = mapper.Map<GlobalSearchItemModel[]>(recent)
            };
        }

        var rows = await repository.SearchAsync(
            currentUser.Id, isSuperAdmin, search, request.Take, CultureHelper.CurrentLanguage, cancellationToken);

        var groups = rows.ToLookup(row => row.ItemType);
        return new GlobalSearchModel
        {
            Projects = MapGroup(groups[GlobalSearchItemType.Project], request.Take),
            Tickets = MapGroup(groups[GlobalSearchItemType.Ticket], request.Take),
            Tasks = MapGroup(groups[GlobalSearchItemType.Task], request.Take),
            Subtasks = MapGroup(groups[GlobalSearchItemType.Subtask], request.Take)
        };
    }

    private GlobalSearchGroupModel MapGroup(IEnumerable<GlobalSearchRow> rows, int take)
    {
        var items = rows.ToArray();
        return new GlobalSearchGroupModel
        {
            Items = mapper.Map<GlobalSearchItemModel[]>(items.Take(take).ToArray()),
            HasMore = items.Length > take
        };
    }
}
