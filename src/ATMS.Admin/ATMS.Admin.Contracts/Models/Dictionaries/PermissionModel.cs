using ATMS.Application.Models;

namespace ATMS.Admin.Contracts.Models.Dictionaries;

public sealed class PermissionModel : DictionaryModel
{
    public string Module { get; set; }    
}
