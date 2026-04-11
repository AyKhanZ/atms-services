using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class Role : BaseEntity, ISoftDeletable
{
    public string Name { get; set; }
    
    public string Description { get; set; }


    public ICollection<WorkProjectParticipantRole> WorkProjectParticipantRoles { get; set; } = [];
    
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    
    
    public bool IsDeleted { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public Guid? DeletedById { get; set; }
}
