using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class WorkProjectParticipant : BaseEntity, ISoftDeletable
{
    public Guid UserId { get; set; }
    
    public User User { get; set; }
    
    
    public WorkProject WorkProject { get; set; }
    
    public Guid WorkProjectId { get; set; }


    public ICollection<WorkProjectParticipantRole> WorkProjectParticipantRoles { get; set; } = [];
    
    
    public bool IsDeleted { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public Guid? DeletedById { get; set; }
}
