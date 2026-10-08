using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.WorkProjects;

public sealed class WorkProjectParticipantModel : AuditUserModel
{
    public Guid UserId { get; set; }

    public string Email { get; set; }

    public string? AvatarPath { get; set; }

    public bool HasCompletedOnboarding { get; set; }

    public string Category { get; set; }

    public WorkProjectRoleModel Role { get; set; }
}
