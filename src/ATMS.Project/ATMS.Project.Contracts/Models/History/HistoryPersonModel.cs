using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.History;

public class HistoryPersonModel : AuditUserModel
{
    public string? AvatarPath { get; set; }
}
