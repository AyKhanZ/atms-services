using ATMS.Application.Models;

namespace ATMS.Admin.Contracts.Models.Users;

public class UserListItemModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public string Patronymic { get; set; }
    
    public string Email { get; set; }
    

    public DictionaryModel? UserStatus { get; set; }
    
    
    public string AvatarPath { get; set; }
    
    public string? Position { get; set; }
}
