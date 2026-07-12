using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.Organization;
using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Data.Criteria.Organizations;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Organizations;

public class GetOrganizationsHandler(
    IOrganizationRepository organizationRepository,
    IMapper mapper)
    : IRequestHandler<GetOrganizationsRequest, PagedResult<OrganizationItemModel>>
{
    public async Task<PagedResult<OrganizationItemModel>> Handle(GetOrganizationsRequest request,
        CancellationToken cancellationToken)
    {
        var filter = mapper.Map<OrganizationFilter>(request);
        
        var pagination = new PaginationCriteria<Organization>(request.Page, request.PageSize);
        
        var organizations = await organizationRepository.GetAsync(filter, pagination, cancellationToken);

        return organizations.Map(mapper.Map<OrganizationItemModel>);
    }
}