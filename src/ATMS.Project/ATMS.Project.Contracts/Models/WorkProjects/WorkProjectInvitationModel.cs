using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.WorkProjects;

public sealed class WorkProjectInvitationModel : AuditUserModel
{
    public string Email { get; set; }

    public WorkProjectRoleModel Role { get; set; }

    public DateTime CreatedAt { get; set; }
}
