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
    IProjectPermissionService projectPermissionService,
    IProjectAccessPolicyResolver projectAccessPolicyResolver)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ProjectAccessAttribute[] AccessRequirements = typeof(TRequest)
        .GetCustomAttributes(typeof(ProjectAccessAttribute), inherit: false)
        .Cast<ProjectAccessAttribute>()
        .ToArray();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (currentUser.RoleId == RoleIds.SuperAdmin)
        {
            return await next(cancellationToken);
        }

        if (AccessRequirements.Length == 0)
        {
            return await next(cancellationToken);
        }

        if (request is not IProjectScopedRequest projectRequest)
        {
            throw new AuthException(AuthErrorType.Forbidden, ExceptionMessages.ProjectAccessDenied);
        }

        if (projectRequest.ProjectId == Guid.Empty)
        {
            throw new AuthException(AuthErrorType.Forbidden, ExceptionMessages.ProjectAccessDenied);
        }

        var requirements = await ResolveRequirementsAsync(projectRequest, cancellationToken);
        var permissionCodes = await projectPermissionService.GetPermissionCodesAsync(
            projectRequest.ProjectId,
            cancellationToken);

        var hasAccess = requirements.All(requirement =>
            requirement.Count > 0 &&
            requirement.Any(permission => permissionCodes.Contains(permission.ToString())));

        if (!hasAccess)
        {
            Deny();
        }

        return await next(cancellationToken);
    }

    private static IReadOnlyCollection<ProjectPermissionEnum>[] StaticRequirements()
    {
        return AccessRequirements
            .Where(attribute => !attribute.Policy.HasValue)
            .Select(attribute => attribute.Permissions)
            .ToArray();
    }

    private async Task<IReadOnlyCollection<ProjectPermissionEnum>[]> ResolveRequirementsAsync(
        IProjectScopedRequest request,
        CancellationToken cancellationToken)
    {
        var staticRequirements = StaticRequirements();
        var policyRequirements = new List<IReadOnlyCollection<ProjectPermissionEnum>>();

        foreach (var attribute in AccessRequirements)
        {
            if (attribute.Policy is not ProjectAccessPolicy policy)
            {
                continue;
            }

            var permissions = await projectAccessPolicyResolver.ResolveAsync(
                policy,
                request,
                cancellationToken);

            policyRequirements.Add(permissions);
        }

        return staticRequirements
            .Concat(policyRequirements)
            .ToArray();
    }

    [DoesNotReturn]
    private static void Deny() =>
        throw new AuthException(AuthErrorType.Forbidden, ExceptionMessages.ProjectAccessDenied);
}
