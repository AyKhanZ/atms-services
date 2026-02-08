using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers;

public class EntityToModelProfile : Profile
{
    public EntityToModelProfile()
    {
        CreateMap<Role, RoleModel>();
    }
}
