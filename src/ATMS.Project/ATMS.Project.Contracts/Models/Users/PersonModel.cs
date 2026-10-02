using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.Users;

public class PersonModel : AuditUserModel
{
    public string? AvatarPath { get; set; }
}
