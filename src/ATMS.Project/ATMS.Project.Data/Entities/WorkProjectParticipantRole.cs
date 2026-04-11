using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class WorkProjectParticipantRole : BaseEntity, ISoftDeletable
{
    public Guid WorkProjectParticipantId { get; set; }
    
    public WorkProjectParticipant WorkProjectParticipant { get; set; }
    
    
    public Guid RoleId { get; set; }
    
    public Role Role { get; set; }
    
    
    public bool IsDeleted { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public Guid? DeletedById { get; set; }
}
