using ATMS.Admin.Data.Entities;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers;

public class EventToEntityProfile : Profile
{
    public EventToEntityProfile()
    {
        CreateMap<UserInvitedEvent, User>(MemberList.None)
            .ForMember(destination => destination.Email,
                options => options.MapFrom(source => source.Email.Trim()))
            .ForMember(destination => destination.NormalizedEmail,
                options => options.MapFrom(source => source.Email.Trim().ToUpperInvariant()))
            .ForMember(destination => destination.Name,
                options => options.MapFrom(source => source.Name.Trim()))
            .ForMember(destination => destination.Surname,
                options => options.MapFrom(source => source.Surname.Trim()))
            .ForMember(destination => destination.OrganizationId,
                options => options.MapFrom(source => source.OrganizationId))
            .ForMember(destination => destination.InvitedById,
                options => options.MapFrom(source => source.InvitedByUserId))
            .ForMember(destination => destination.AvatarPath,
                options => options.MapFrom(_ => DefaultValues.UserAvatar))
            .ForMember(destination => destination.LanguageId,
                options => options.MapFrom(_ => DefaultValues.Language));
    }
}
