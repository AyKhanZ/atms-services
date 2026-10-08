using ATMS.Data.Constants;
using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.Search;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Search;

public sealed class RecordGlobalSearchRecentHandler(ICurrentUser currentUser, IGlobalSearchRecentRepository repository)
    : IRequestHandler<RecordGlobalSearchRecentCommand>
{
    public Task Handle(RecordGlobalSearchRecentCommand request, CancellationToken cancellationToken)
    {
        // an item the user can't see is just not saved, it's a background ping, nothing to report
        return repository.RecordRecentAsync(
            currentUser.Id,
            currentUser.RoleId == RoleIds.SuperAdmin,
            (GlobalSearchItemTypeEnum)request.ItemType,
            request.ItemId,
            cancellationToken);
    }
}
