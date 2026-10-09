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

    [Theory]
    [InlineData(null)]
    [InlineData("Operations manager")]
    public void MapCreatedEvent_CopiesPosition(string? position)
    {
        var message = new UserCreatedEvent(
            Guid.NewGuid(),
            "user@baim.az",
            "Aykhan",
            "Zeynalov",
            1,
            "avatar.png",
            Guid.NewGuid(),
            Position: position);

        var user = CreateMapper().Map<User>(message);

        Assert.Equal(position, user.Position);
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

    // Organization details show the position; it changes in Admin and arrives only through this event.
    [Theory]
    [InlineData("Old", "Operations manager")]
    [InlineData("Old", null)]
    public void MapUpdatedEvent_ReplacesPosition(string current, string? updated)
    {
        var user = new User { Id = Guid.NewGuid(), Position = current };
        var message = new UserUpdatedEvent(user.Id, "Aykhan", "Zeynalov", "avatar.png", true, updated);

        CreateMapper().Map(message, user);

        Assert.Equal(updated, user.Position);
    }

    [Theory]
    [InlineData("ru")]
    [InlineData(null)]
    public void MapUpdatedEvent_CopiesLanguage(string? language)
    {
        var user = new User { Id = Guid.NewGuid(), Language = "en" };
        var message = new UserUpdatedEvent(user.Id, "Aykhan", "Zeynalov", "avatar.png", true, Language: language);

        CreateMapper().Map(message, user);

        Assert.Equal(language, user.Language);
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
