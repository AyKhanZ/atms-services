using ATMS.Data;

namespace ATMS.Admin.Data.Entities.UserProgress;

public class InvitedUser : UserBase
{
    public Guid UserProgressId { get; set; }
    
    public UserProgress UserProgress { get; set; }
}
