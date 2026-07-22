using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Data.Criteria.Organizations;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class RequestToFilterProfile : Profile
{
    public RequestToFilterProfile()
    {
        CreateMap<GetOrganizationsRequest, OrganizationFilter>();
    }
}
