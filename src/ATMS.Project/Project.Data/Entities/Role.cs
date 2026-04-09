using ATMS.Data;

namespace Project.Data.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; }
    
    public string Description { get; set; }

    
    public ICollection<WorkProjectParticipantRole> WorkProjectParticipantRoles { get; set; }
    
    public ICollection<RolePermission> RolePermissions { get; set; }
}
