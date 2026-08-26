using ATMS.Application.Interfaces;
using ATMS.Project.Services.Security.Interfaces;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.WorkProjects;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class GetMyProjectPermissionsHandler(
    ICurrentUser currentUser,
    IProjectPermissionService permissionService) : IRequestHandler<GetMyProjectPermissionsRequest, string[]>
{
    public async Task<string[]> Handle(GetMyProjectPermissionsRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.RoleId == RoleIds.SuperAdmin)
        {
            return Enum.GetNames<ProjectPermissionEnum>();
        }

        var permissions = await permissionService.GetPermissionCodesAsync(
            request.ProjectId,
            cancellationToken);

        return permissions.Order(StringComparer.Ordinal).ToArray();
    }
}
