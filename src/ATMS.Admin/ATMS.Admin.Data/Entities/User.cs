using ATMS.Admin.Data.Entities.Dictionaries;

namespace ATMS.Admin.Data.Entities;

public class User : UserBase
{
    public string? Patronymic { get; set; }
    
    public string? PhoneNumber { get; set; }

    public string? PersonalEmail { get; set; }

    public string? PersonalPhoneNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    public string AvatarPath { get; set; }

    public string? Position { get; set; }


    public string PasswordHash { get; set; }

    public bool HasCompletedSurvey { get; set; }

    public bool EmailConfirmed { get; set; }

    public uint FailedLoginCount { get; set; }

    public DateTime? LockoutEnd { get; set; }


    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    
    #region Dictionaries
    public int UserStatusId { get; set; }

    public UserStatus UserStatus { get; set; }

    public int MaritalStatusId { get; set; }

    public MaritalStatus MaritalStatus { get; set; }

    public int GenderId { get; set; }

    public Gender Gender { get; set; }
    #endregion


    public List<UserRole> UserRoles { get; set; } = [];
}
