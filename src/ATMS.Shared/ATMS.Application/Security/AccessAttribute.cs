using ATMS.Data.Enums;

namespace ATMS.Application.Security;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class AccessAttribute(PermissionEnum permission) : Attribute
{
    public PermissionEnum Permission { get; } = permission;
}
