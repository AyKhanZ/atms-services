using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.Users;

/// <summary>A person shown beside what they did — in history, comments and mentions.</summary>
public class PersonModel : AuditUserModel
{
    public string? AvatarPath { get; set; }
}
