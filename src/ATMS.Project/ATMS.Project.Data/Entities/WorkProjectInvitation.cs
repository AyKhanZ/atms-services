using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class WorkProjectInvitation : UserBase
{
    public Guid WorkProjectId { get; set; }

    public WorkProject WorkProject { get; set; }

    public string NormalizedEmail { get; set; }

    public Guid RoleId { get; set; }

    public Role Role { get; set; }

    public int Status { get; set; }

    public Guid InvitedById { get; set; }

    public User InvitedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
