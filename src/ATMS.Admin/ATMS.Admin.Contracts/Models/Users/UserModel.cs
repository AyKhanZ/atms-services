using ATMS.Admin.Contracts.Models.Organizations;
using ATMS.Application.Models;

namespace ATMS.Admin.Contracts.Models.Users;

public sealed class UserModel : AuditUserModel
{ 
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    
    public DateTime? BirthDate { get; set; }


    public DictionaryModel<Guid>[] Roles { get; set; } = [];
    public DictionaryModel? Gender { get; set; }
    
    public DictionaryModel? MaritalStatus { get; set; }
    
    
    public DictionaryModel? UserStatus { get; set; }
    
    public DateTime? LockoutEnd { get; set; }

    public DateTime CreatedAt { get; set; }

    
    public string AvatarPath { get; set; }
    
    public string? Position { get; set; }

    public OrganizationModel? Organization { get; set; }


    public bool HasCompletedOnboarding { get; set; }
    
    public bool EmailConfirmed { get; set; }
}
