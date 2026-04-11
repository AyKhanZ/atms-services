using ATMS.Project.Contracts.Models.Organization;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Organizations;

public class GetOrganizationsRequest : IRequest<OrganizationItemModel[]>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
