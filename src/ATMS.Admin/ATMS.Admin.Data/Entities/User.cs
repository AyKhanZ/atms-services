namespace ATMS.Admin.Data.Entities;

public class User : UserBase
{
    public string? Patronymic { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PersonalEmail { get; set; }
    public string? PersonalPhoneNumber { get; set; }

    public DateTime BirthDate { get; set; }
    public string Gender { get; set; }

    public string AvatarPath { get; set; }
    public string? Position { get; set; }


    public bool HasCompletedSurvey { get; set; }


    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public DateTime RefreshTokenCreatedAt { get; set; }


    public List<UserRole> UserRoles { get; set; }
}
