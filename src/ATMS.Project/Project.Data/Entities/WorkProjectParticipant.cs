using ATMS.Data;

namespace Project.Data.Entities;

public class WorkProjectParticipant : BaseEntity
{
    public Guid UserId { get; set; }
    
    public User User { get; set; }
    
    
    public WorkProject WorkProject { get; set; }
    
    public Guid WorkProjectId { get; set; }
    

    public ICollection<WorkProjectParticipantRole> WorkProjectParticipantRoles { get; set; }
}
