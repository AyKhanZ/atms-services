using ATMS.Project.Contracts.Commands.Organizations;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class CommandToEntityProfile : Profile
{
    public CommandToEntityProfile()
    {
        CreateMap<CreateOrganizationCommand, Organization>();
        CreateMap<UpdateOrganizationCommand, Organization>();

        CreateMap<CreateWorkProjectCommand, WorkProject>();
        CreateMap<UpdateWorkProjectCommand, WorkProject>();
    }
}
