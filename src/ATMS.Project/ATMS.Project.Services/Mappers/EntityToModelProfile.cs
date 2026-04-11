using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class EntityToModelProfile : Profile
{
    // public EntityToModelProfile()
    // {
    //     CreateMap<Role, DictionaryModel<Guid>>()
    //         .ForMember(d => d.Code,
    //             opt => opt.MapFrom(r => r.Name));
    //     CreateMap<Role, RoleModel>()
    //         .ForMember(d => d.Permissions, opt => opt.MapFrom(r =>
    //             r.RolePermissions.Select(rp => new DictionaryModel
    //             {
    //                 Id = rp.Permission.Id,
    //                 Code = rp.Permission.Code,
    //                 Name = rp.Permission.Code
    //             }).ToArray()));
    //     
    //     CreateMap<User, MeModel>();
    //     CreateMap<User, UserModel>()
    //         .ForMember(d => d.Gender, opt => opt.Ignore())
    //         .ForMember(d => d.MaritalStatus, opt => opt.Ignore())
    //         .ForMember(d => d.UserStatus, opt => opt.Ignore())
    //         .ForMember(d => d.Roles, opt => opt.Ignore());
    //     CreateMap<User, UserListItemModel>()
    //         .ForMember(d => d.UserStatus, opt => opt.Ignore());
    // }
}
