using ATMS.Project.Contracts.Models.Organization;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Organizations;

public class GetOrganizationRequest : IRequest<OrganizationModel>
{
    public Guid Id { get; set; }
}
