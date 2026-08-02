using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Entities;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class CommandToEntityProfile : Profile
{
    public CommandToEntityProfile()
    {
        CreateMap<CreateOrganizationCommand, Organization>();
        CreateMap<UpdateOrganizationCommand, Organization>();
    }
}
