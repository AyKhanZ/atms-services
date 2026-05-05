using ATMS.Data;

namespace ATMS.Admin.Data.Entities.UserProgresses;

public class InvitedUser : UserBase
{
    public Guid UserProgressId { get; set; }
}