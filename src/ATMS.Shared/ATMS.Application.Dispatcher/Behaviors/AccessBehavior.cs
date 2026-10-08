using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Constants;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Application.Security;
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

    private static readonly bool ExcludesSuperAdmin = typeof(TRequest)
        .IsDefined(typeof(ExceptSuperAdminAccessAttribute), inherit: false);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // no access attributes, nothing to check
        if (!RequiresSuperAdmin && !ExcludesSuperAdmin && AccessRequirements.Length == 0)
        {
            return await next(cancellationToken);
        }

        // super admin can do everything here
        if (currentUser.RoleId == RoleIds.SuperAdmin)
        {
            if (ExcludesSuperAdmin)
            {
                throw new AuthException(AuthErrorTypeEnum.Forbidden, ExceptionMessages.AccessDenied);
            }

            return await next(cancellationToken);
        }

        // permissions inside one attribute = any of them, several attributes = all of them
        if (RequiresSuperAdmin || !HasAllSystemPermissionRequirements())
        {
            throw new AuthException(AuthErrorTypeEnum.Forbidden, ExceptionMessages.AccessDenied);
        }

        return await next(cancellationToken);
    }

    private bool HasAllSystemPermissionRequirements()
        => AccessRequirements.All(requirement =>
            requirement.Permissions.Any(permission => currentUser.Permissions.Contains(permission.ToString())));
}
