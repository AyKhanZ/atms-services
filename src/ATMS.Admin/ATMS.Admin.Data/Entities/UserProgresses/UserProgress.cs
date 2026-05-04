using ATMS.Data.Enums;

namespace ATMS.Admin.Data.Entities.UserProgresses;

public class UserProgress
{
    public Guid UserId { get; set; }
    
    public User User { get; set; }
    
    
    public Guid RoleId { get; set; }
    
    public UserProgressTypeEnum UserProgressType { get; set; }
    
    
    public ushort CurrentStep { get; set; }

    
    public DateTime LastUpdated { get; set; }


    // Personal
    public Guid? PersonalInfoId { get; set; }
    
    public PersonalInfo? PersonalInfo { get; set; }


    // Security
    public string? PasswordHash { get; set; }


    // Invite
    public List<InvitedUser>? InvitedUsers { get; set; }
}