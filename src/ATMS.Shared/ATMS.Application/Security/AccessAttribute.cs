using ATMS.Data.Enums;

namespace ATMS.Application.Security;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class AccessAttribute(params PermissionEnum[] permissions) : Attribute
{
    public IReadOnlyCollection<PermissionEnum> Permissions { get; } = permissions.Length > 0
        ? permissions.Distinct().ToArray()
        : throw new ArgumentException(@"At least one permission must be specified.", nameof(permissions));
}
