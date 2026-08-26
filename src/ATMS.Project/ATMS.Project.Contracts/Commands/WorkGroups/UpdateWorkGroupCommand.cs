using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkGroups;

[ProjectAccess(ProjectPermissionEnum.GroupEdit)]
public class UpdateWorkGroupCommand : WorkGroupCommand, IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid WorkGroupId { get; set; }
}
