using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.Organization;

public class OrganizationItemModel : OrganizationModel
{
    public UserModel[] Users { get; set; }
}
