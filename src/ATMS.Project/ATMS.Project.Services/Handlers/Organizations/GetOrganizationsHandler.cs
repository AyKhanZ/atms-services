using ATMS.Project.Contracts.Models.Organization;
using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Organizations;

public class GetOrganizationsHandler(
    IOrganizationRepository organizationRepository,
    IMapper mapper)
    : IRequestHandler<GetOrganizationsRequest, OrganizationItemModel[]>
{
    public async Task<OrganizationItemModel[]> Handle(GetOrganizationsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await organizationRepository.GetAsync(cancellationToken);
        
        return mapper.Map<OrganizationItemModel[]>(result);
    }
}