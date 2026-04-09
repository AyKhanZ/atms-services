using ATMS.Data;

namespace Project.Data.Entities;

public class WorkProjectParticipantRole : BaseEntity
{
    public Guid WorkProjectParticipantId { get; set; }
    
    public WorkProjectParticipant WorkProjectParticipant { get; set; }
    
    
    public Guid RoleId { get; set; }
    
    public Role Role { get; set; }
}
