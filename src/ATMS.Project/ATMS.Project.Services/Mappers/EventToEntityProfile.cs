using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Project.Data.Entities;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class EventToEntityProfile : Profile
{
    public EventToEntityProfile()
    {
        CreateMap<UserCreatedEvent, User>()
            .ForMember(destination => destination.AvatarPath,
                options => options.MapFrom(source => string.IsNullOrWhiteSpace(source.AvatarPath)
                    ? DefaultValues.UserAvatar
                    : source.AvatarPath))
            .ForMember(destination => destination.Organization, options => options.Ignore());

        CreateMap<UserUpdatedEvent, User>();
    }
}
