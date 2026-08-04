namespace ATMS.Project.Contracts.Models.WorkProjects;

public class WorkProjectParticipantModel
{
    public Guid UserId { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Email { get; set; }

    public WorkProjectRoleModel Role { get; set; }
}
