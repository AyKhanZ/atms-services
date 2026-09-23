using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.WorkTaskBoard;

public class WorkTaskBoardAssigneeModel : AuditUserModel
{
    public string? AvatarPath { get; set; }
}
