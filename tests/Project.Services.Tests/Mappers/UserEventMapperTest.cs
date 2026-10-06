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
        var mapper = CreateMapper();
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
        Assert.Equal("USER@BAIM.AZ", user.NormalizedEmail);
        Assert.False(user.IsAdmin);
        Assert.False(user.HasCompletedOnboarding);
        Assert.Equal(DefaultValues.UserAvatar, user.AvatarPath);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MapCreatedEvent_CopiesOnboardingFlag(bool hasCompletedOnboarding)
    {
        var message = new UserCreatedEvent(
            Guid.NewGuid(),
            "user@baim.az",
            "Aykhan",
            "Zeynalov",
            1,
            "avatar.png",
            Guid.NewGuid(),
            HasCompletedOnboarding: hasCompletedOnboarding);

        var user = CreateMapper().Map<User>(message);

        Assert.Equal(hasCompletedOnboarding, user.HasCompletedOnboarding);
    }

    // The participant chip "Invited" goes away when Admin says onboarding is done.
    [Fact]
    public void MapUpdatedEvent_MarksOnboardingCompleted()
    {
        var user = new User { Id = Guid.NewGuid(), HasCompletedOnboarding = false };
        var message = new UserUpdatedEvent(user.Id, "Aykhan", "Zeynalov", "avatar.png", true);

        CreateMapper().Map(message, user);

        Assert.True(user.HasCompletedOnboarding);
        Assert.Equal("Aykhan", user.Name);
    }

    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMapper>();
    }
}
