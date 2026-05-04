namespace ATMS.Admin.Contracts.Models.UserProgresses;

public class UserProgressModel
{
    public string UserProgressType { get; set; }

    public ushort CurrentStep { get; set; }
    
    public DateTime? LastUpdated { get; set; }
    
    
    public string? PasswordHash { get; set; }
    
    public InvitedUsersModel[]? InvitedUsers { get; set; }
    
    public PersonalInfoModel? PersonalInfo { get; set; }
}