using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.Users;

public sealed class UserModel : AuditUserModel
{
    public string Email { get; set; }

    public string? AvatarPath { get; set; }

    public string? Position { get; set; }
}
