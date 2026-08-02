using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Modules;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Services.Tests.Mappers;

public sealed class UserEventMapperTest
{
    [Fact]
    public void MapCreatedEvent_UsesDefaultAvatarWhenMessageDoesNotContainOne()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var message = new UserCreatedEvent(
            Guid.NewGuid(),
            "user@baim.az",
            "Aykhan",
            "Zeynalov",
            1,
            " ",
            Guid.NewGuid());

        var user = mapper.Map<User>(message);

        Assert.Equal(message.Id, user.Id);
        Assert.Equal(message.Email, user.Email);
        Assert.Equal(DefaultValues.UserAvatar, user.AvatarPath);
    }
}
