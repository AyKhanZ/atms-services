using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace ATMS.Application.Dispatcher.Behaviors;

public sealed class AccessBehavior<TRequest, TResponse>(
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly AccessAttribute[] AccessRequirements = typeof(TRequest)
        .GetCustomAttributes(typeof(AccessAttribute), inherit: false)
        .Cast<AccessAttribute>()
        .ToArray();

    private static readonly bool RequiresSuperAdmin = typeof(TRequest)
        .IsDefined(typeof(SuperAdminAccessAttribute), inherit: false);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Requests without SuperAdminAccess or Access attributes do not require system permission checks.
        if (!RequiresSuperAdmin && AccessRequirements.Length == 0)
        {
            return await next(cancellationToken);
        }

        // SuperAdmin has access to every system-level operation.
        if (currentUser.RoleId == RoleIds.SuperAdmin)
        {
            return await next(cancellationToken);
        }

        // At this point SuperAdmin was already allowed above.
        // If this request explicitly requires SuperAdmin, any other user must be denied.
        // Permissions inside one attribute are alternatives. Multiple attributes are cumulative requirements.
        if (RequiresSuperAdmin || !HasAllSystemPermissionRequirements())
        {
            Deny();
        }

        return await next(cancellationToken);
    }

    private bool HasAllSystemPermissionRequirements()
        => AccessRequirements.All(requirement =>
            requirement.Permissions.Any(permission => currentUser.Permissions.Contains(permission.ToString())));

    [DoesNotReturn]
    private static void Deny() => throw new AuthException(AuthErrorType.Forbidden, ExceptionMessages.AccessDenied);
}
