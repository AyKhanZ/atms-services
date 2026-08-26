using ATMS.Data.Enums;

namespace ATMS.Application.Security;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class ProjectAccessAttribute(ProjectPermissionEnum permission) : Attribute
{
    public ProjectPermissionEnum Permission { get; } = permission;
}
