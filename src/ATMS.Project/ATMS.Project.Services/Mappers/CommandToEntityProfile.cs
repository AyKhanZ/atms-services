using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Data.Criterias.Organizations;
using ATMS.Project.Data.Entities;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class CommandToEntityProfile : Profile
{
    public CommandToEntityProfile()
    {
        CreateMap<CreateOrganizationCommand, Organization>();
        CreateMap<UpdateOrganizationCommand, Organization>();
        
        CreateMap<GetOrganizationsRequest, OrganizationFilter>();
    }
}
