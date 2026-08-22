using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Contracts.Requests.WorkProjects;
using ATMS.Project.Data.Criteria.Organizations;
using ATMS.Project.Data.Criteria.WorkProjects;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class RequestToFilterProfile : Profile
{
    public RequestToFilterProfile()
    {
        CreateMap<GetOrganizationsRequest, OrganizationFilter>();
        CreateMap<GetWorkProjectsRequest, WorkProjectsFilter>();
    }
}
