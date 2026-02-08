using ATMS.Admin.Contracts.Commands;
using ATMS.Admin.Data.Entities;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers;

public class CommandToEntityProfile : Profile
{
    public CommandToEntityProfile()
    {
        CreateMap<CreateRoleCommand, Role>();
        CreateMap<UpdateRoleCommand, Role>();
    }
}
