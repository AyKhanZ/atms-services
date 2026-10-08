using ATMS.Application.Models;

namespace ATMS.Admin.Contracts.Models.Me;

public sealed class MeModel : AuditUserModel
{
    public string Language { get; set; }
    
    public string AvatarPath { get; set; }
}
