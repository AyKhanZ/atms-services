namespace ATMS.Admin.Data.Entities;

public class UserProgress
{
    public Guid UserId { get; set; }
    public UserBase User { get; set; }

    public ushort CurrentStep { get; set; }


    public Guid? PersonalInfoId { get; set; }
    public PersonalInfo? PersonalInfo { get; set; }


    public string? PasswordHash { get; set; }


    public Guid? OrganizationInfoId { get; set; }
    public OrganizationInfo? OrganizationInfo { get; set; }


    public List<InvitedUser>? InvitedUsers { get; set; }


    public DateTime LastUpdated { get; set; }
}
