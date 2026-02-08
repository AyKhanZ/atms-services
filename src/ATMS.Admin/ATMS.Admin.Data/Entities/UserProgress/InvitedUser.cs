namespace ATMS.Admin.Data.Entities;

public class InvitedUser : UserBase
{
    public Guid UserProgressId { get; set; }
    public UserProgress UserProgress { get; set; }
}
