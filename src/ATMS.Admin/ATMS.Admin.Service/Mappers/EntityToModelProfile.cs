using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Application.Models;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers;

public class EntityToModelProfile : Profile
{
    public EntityToModelProfile()
    {
        CreateMap<Role, DictionaryModel<Guid>>()
            .ForMember(d => d.Code,
                opt => opt.MapFrom(r => r.Name));
        CreateMap<Role, RoleModel>();
        CreateMap<User, UserModel>();
    }
}
