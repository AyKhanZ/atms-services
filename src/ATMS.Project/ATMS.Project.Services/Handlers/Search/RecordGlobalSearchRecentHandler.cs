using ATMS.Data.Constants;
using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.Search;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Search;

public class RecordGlobalSearchRecentHandler(ICurrentUser currentUser, IGlobalSearchRecentRepository repository)
    : IRequestHandler<RecordGlobalSearchRecentCommand>
{
    public Task Handle(RecordGlobalSearchRecentCommand request, CancellationToken cancellationToken)
    {
        // An item the user cannot see is simply not written. The caller is a background ping sent
        // while a page opens, so a missing entry is nothing to report and nothing to act on.
        return repository.RecordRecentAsync(
            currentUser.Id,
            currentUser.RoleId == RoleIds.SuperAdmin,
            (GlobalSearchItemType)request.ItemType,
            request.ItemId,
            cancellationToken);
    }
}
