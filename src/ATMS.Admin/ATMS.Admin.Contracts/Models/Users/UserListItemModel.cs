using ATMS.Application.Models;

namespace ATMS.Admin.Contracts.Models.Users;

public sealed class UserListItemModel : AuditUserModel
{
    public string Email { get; set; }
    

    public DictionaryModel? UserStatus { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string AvatarPath { get; set; }
    
    public string? Position { get; set; }
}
