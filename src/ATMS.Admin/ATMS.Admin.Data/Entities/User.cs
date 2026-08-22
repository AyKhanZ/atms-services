using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Admin.Data.Entities;

public class User : UserBase, IAuditable
{
    public string? PhoneNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    public string AvatarPath { get; set; }

    public string? Position { get; set; }


    public string PasswordHash { get; set; }

    public bool HasCompletedOnboarding { get; set; }

    public DateTime? OnboardingCompletedAt { get; set; }

    public bool EmailConfirmed { get; set; }

    public uint FailedLoginCount { get; set; }

    public DateTime? LockoutEnd { get; set; }


    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    
    public int LanguageId { get; set; }

    public Language Language { get; set; }

    public string NormalizedEmail { get; set; }
    
    
    
    public Guid? OrganizationId { get; set; }
    
    public bool IsAdmin { get; set; }
    
    public Guid? InvitedById { get; set; }

    public User? InvitedBy { get; set; }
    
    public DateTime? LastLogin { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedById { get; set; }

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
