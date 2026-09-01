using ATMS.Data.Enums;

namespace ATMS.Application.Security;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class ProjectAccessAttribute : Attribute
{
    public ProjectAccessAttribute(params ProjectPermissionEnum[] permissions)
    {
        Permissions = permissions.Length > 0
            ? permissions.Distinct().ToArray()
            : throw new ArgumentException(@"At least one permission must be specified.", nameof(permissions));
    }

    public ProjectAccessAttribute(ProjectAccessPolicy policy)
    {
        Policy = policy;
        Permissions = [];
    }

    public IReadOnlyCollection<ProjectPermissionEnum> Permissions { get; }

    public ProjectAccessPolicy? Policy { get; }
}
