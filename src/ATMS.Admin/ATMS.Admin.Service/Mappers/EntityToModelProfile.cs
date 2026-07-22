using ATMS.Admin.Contracts.Models.Dictionaries;
using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Mappers.Actions;
using ATMS.Application.Models;
using ATMS.Data.Enums;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers;

public class EntityToModelProfile : Profile
{
    public EntityToModelProfile()
    {
        CreateMap<Role, DictionaryModel<Guid>>()
            .ForMember(destination => destination.Code,
                options => options.MapFrom(source => source.Name));
        
        CreateMap<Language, LanguageModel>();
        
        
        CreateMap<OnboardingInvitedUser, OnboardingInvitedUserModel>();
        
        CreateMap<OnboardingProgress, OnboardingPersonalInfoModel>()
            .ForMember(destination => destination.Name,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null ? source.PersonalInfo.Name : source.User.Name))
            .ForMember(destination => destination.Surname,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null ? source.PersonalInfo.Surname : source.User.Surname))
            .ForMember(destination => destination.Email,
                options => options.MapFrom(source => source.User.Email))
            .ForMember(destination => destination.PhoneNumber,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null
                        ? source.PersonalInfo.PhoneNumber
                        : source.User.PhoneNumber))
            .ForMember(destination => destination.Position,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null ? source.PersonalInfo.Position : source.User.Position))
            .ForMember(destination => destination.LanguageId,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null
                        ? source.PersonalInfo.LanguageId
                        : source.User.LanguageId))
            .ForMember(destination => destination.AvatarPath,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null
                        ? source.PersonalInfo.AvatarPath
                        : source.User.AvatarPath))
            .ForMember(destination => destination.AvatarUploaded,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null &&
                    !string.IsNullOrWhiteSpace(source.PersonalInfo.AvatarPath)))
            .ForMember(destination => destination.BirthDate,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null
                        ? source.PersonalInfo.BirthDate
                        : source.User.BirthDate.HasValue
                            ? DateOnly.FromDateTime(source.User.BirthDate.Value)
                            : (DateOnly?)null))
            .ForMember(destination => destination.GenderId,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null
                        ? source.PersonalInfo.GenderId
                        : source.User.GenderId))
            .ForMember(destination => destination.MaritalStatusId,
                options => options.MapFrom(source =>
                    source.PersonalInfo != null
                        ? source.PersonalInfo.MaritalStatusId
                        : source.User.MaritalStatusId));
        
        CreateMap<OnboardingProgress, OnboardingModel>()
            .ForMember(destination => destination.Role,
                options => options.Ignore())
            .ForMember(destination => destination.CurrentStep,
                options => options.Ignore())
            .ForMember(destination => destination.SecurityCompleted,
                options => options.MapFrom(source =>
                    source.SecurityStatus == OnboardingStepStatusEnum.Completed))
            .ForMember(destination => destination.Steps,
                options => options.Ignore())
            .ForMember(destination => destination.PersonalInfo,
                options => options.MapFrom(source => source))
            .ForMember(destination => destination.InvitedUsers,
                options => options.MapFrom(source =>
                    source.InvitedUsers.OrderBy(x => x.Name).ThenBy(x => x.Surname)))
            .ForMember(destination => destination.MaxInvitations,
                options => options.Ignore())
            .AfterMap<OnboardingModelMappingAction>();
        
        
        CreateMap<User, MeModel>();
        
        CreateMap<User, UserModel>()
            .ForMember(destination => destination.Gender, options => options.Ignore())
            .ForMember(destination => destination.MaritalStatus, options => options.Ignore())
            .ForMember(destination => destination.UserStatus, options => options.Ignore())
            .ForMember(destination => destination.Roles, options => options.Ignore());
        
        CreateMap<User, UserListItemModel>()
            .ForMember(destination => destination.UserStatus, options => options.Ignore());
    }
}
