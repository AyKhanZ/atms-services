using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkGroups;

[ProjectAccess(ProjectPermissionEnum.GroupDelete)]
public class DeleteWorkGroupCommand : IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid WorkGroupId { get; set; }
}
