using ATMS.Project.Contracts.Models.Organization;
using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Organizations;

public class GetOrganizationHandler(
    IOrganizationRepository organizationRepository,
    IMapper mapper)
    : IRequestHandler<GetOrganizationRequest, OrganizationModel>
{
    public async Task<OrganizationModel> Handle(GetOrganizationRequest request, CancellationToken cancellationToken)
    {
        var entity = await organizationRepository
            .GetAsync(o => o.Id == request.Id, cancellationToken);
        
        return mapper.Map<OrganizationModel>(entity);
    }
}