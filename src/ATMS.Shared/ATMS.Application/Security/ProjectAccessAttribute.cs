using ATMS.Application.Enums;
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

    public ProjectAccessAttribute(ProjectAccessPolicyEnum policy)
    {
        Policy = policy;
        Permissions = [];
    }

    public IReadOnlyCollection<ProjectPermissionEnum> Permissions { get; }

    public ProjectAccessPolicyEnum? Policy { get; }
}
