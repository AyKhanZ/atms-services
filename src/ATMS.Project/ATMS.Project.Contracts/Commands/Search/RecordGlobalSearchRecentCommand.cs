using ATMS.Application.Security;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Project.Contracts.Commands.Search;

[Access(PermissionEnum.ProjectView)]
public class RecordGlobalSearchRecentCommand : IRequest
{
    public int ItemType { get; set; }
    public Guid ItemId { get; set; }
}
