using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Contracts.Models.UserProgresses;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.UserProgresses;
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
        CreateMap<User, MeModel>();
        CreateMap<User, UserModel>()
            .ForMember(d => d.Gender, opt => opt.Ignore())
            .ForMember(d => d.MaritalStatus, opt => opt.Ignore())
            .ForMember(d => d.UserStatus, opt => opt.Ignore())
            .ForMember(d => d.Roles, opt => opt.Ignore());
        CreateMap<User, UserListItemModel>()
            .ForMember(d => d.UserStatus, opt => opt.Ignore());

        CreateMap<UserProgress, UserProgressModel>();
        CreateMap<PersonalInfo, PersonalInfoModel>();
        CreateMap<InvitedUser, InvitedUsersModel>();
    }
}
