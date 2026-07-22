using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers;

public class CommandToEntityProfile : Profile
{
    public CommandToEntityProfile()
    {
        CreateMap<RegisterCommand, User>();

        CreateMap<SavePersonalInfoCommand, OnboardingPersonalInfo>()
            .ForMember(destination => destination.Name,
                options => options.MapFrom(source => source.Name.Trim()))
            .ForMember(destination => destination.Surname,
                options => options.MapFrom(source => source.Surname.Trim()))
            .ForMember(destination => destination.PhoneNumber,
                options => options.MapFrom(source => source.PhoneNumber.Trim()))
            .ForMember(destination => destination.Position,
                options => options.MapFrom(source => source.Position.Trim()))
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Email, options => options.Ignore())
            .ForMember(destination => destination.AvatarPath, options => options.Ignore())
            .ForMember(destination => destination.Progress, options => options.Ignore())
            .ForMember(destination => destination.Language, options => options.Ignore())
            .ForMember(destination => destination.Gender, options => options.Ignore())
            .ForMember(destination => destination.MaritalStatus, options => options.Ignore());

        CreateMap<InvitedUserCommand, OnboardingInvitedUser>()
            .ForMember(destination => destination.Name,
                options => options.MapFrom(source => source.Name.Trim()))
            .ForMember(destination => destination.Surname,
                options => options.MapFrom(source => source.Surname.Trim()))
            .ForMember(destination => destination.Email,
                options => options.MapFrom(source => source.Email.Trim()))
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.OnboardingUserId, options => options.Ignore())
            .ForMember(destination => destination.NormalizedEmail, options => options.Ignore())
            .ForMember(destination => destination.Progress, options => options.Ignore());

        CreateMap<OnboardingPersonalInfo, User>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Email, options => options.Ignore())
            .ForMember(destination => destination.BirthDate,
                options => options.MapFrom(source => source.BirthDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(destination => destination.Language, options => options.Ignore())
            .ForMember(destination => destination.Gender, options => options.Ignore())
            .ForMember(destination => destination.MaritalStatus, options => options.Ignore())
            .ForMember(destination => destination.UserRoles, options => options.Ignore());
    }
}
