using ATMS.Application.Models;

namespace ATMS.Admin.Contracts.Models;

public class RoleModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public DictionaryModel[] Permissions { get; set; }
}
