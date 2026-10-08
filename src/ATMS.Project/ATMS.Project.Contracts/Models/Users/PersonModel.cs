using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.Users;

public sealed class PersonModel : AuditUserModel
{
    public string? AvatarPath { get; set; }
}
