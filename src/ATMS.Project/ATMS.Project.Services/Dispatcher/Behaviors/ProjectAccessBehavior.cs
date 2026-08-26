using System.Diagnostics.CodeAnalysis;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using ATMS.Project.Services.Security.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Dispatcher.Behaviors;

public sealed class ProjectAccessBehavior<TRequest, TResponse>(
    ICurrentUser currentUser,
    IProjectPermissionService projectPermissionService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ProjectPermissionEnum[] ProjectPermissions = typeof(TRequest)
        .GetCustomAttributes(typeof(ProjectAccessAttribute), inherit: false)
        .Cast<ProjectAccessAttribute>()
        .Select(attribute => attribute.Permission)
        .Distinct()
        .ToArray();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (ProjectPermissions.Length == 0 || currentUser.RoleId == RoleIds.SuperAdmin)
        {
            return await next(cancellationToken);
        }

        if (request is not IProjectScopedRequest)
        {
            Deny();
        }

        var projectRequest = (IProjectScopedRequest)request;
        if (projectRequest.ProjectId == Guid.Empty)
        {
            Deny();
        }

        if (!await projectPermissionService.HasAnyPermissionAsync(
                projectRequest.ProjectId,
                ProjectPermissions,
                cancellationToken))
        {
            Deny();
        }

        return await next(cancellationToken);
    }

    [DoesNotReturn]
    private static void Deny() =>
        throw new AuthException(AuthErrorType.Forbidden, ExceptionMessages.ProjectAccessDenied);
}
