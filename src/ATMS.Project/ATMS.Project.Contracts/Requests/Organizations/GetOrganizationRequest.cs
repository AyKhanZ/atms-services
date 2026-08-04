using ATMS.Project.Contracts.Models.Organizations;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Organizations;

public class GetOrganizationRequest : IRequest<OrganizationModel>
{
    public required Guid Id { get; set; }
}
