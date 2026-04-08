using ATMS.Data;

namespace Project.Data.Entities;

public class MemberRole : BaseEntity
{
    public Guid MemberId { get; set; }
    
    public Member Member { get; set; }
    
    
    public Guid RoleId { get; set; }
    
    public Role Role { get; set; }
}
