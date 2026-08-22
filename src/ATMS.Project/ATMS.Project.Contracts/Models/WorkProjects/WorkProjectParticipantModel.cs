namespace ATMS.Project.Contracts.Models.WorkProjects;

public class WorkProjectParticipantModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Email { get; set; }

    public string? AvatarPath { get; set; }

    public string Category { get; set; }

    public WorkProjectRoleModel Role { get; set; }
}
