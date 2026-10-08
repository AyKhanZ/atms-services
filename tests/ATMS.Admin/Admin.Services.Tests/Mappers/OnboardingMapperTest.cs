using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Modules;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Services.Tests.Mappers;

public sealed class OnboardingMapperTest
{
    [Fact]
    public void MapPersonalInfo_UpdatesProfileWithoutReplacingUserIdentity()
    {
        using var provider = BuildProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "account@baim.az",
            Name = "Old",
            Surname = "Name"
        };
        var personalInfo = new OnboardingPersonalInfo
        {
            Id = Guid.NewGuid(),
            Email = "ignored@baim.az",
            Name = "Aykhan",
            Surname = "Zeynalov",
            PhoneNumber = "+994501112233",
            Position = "Manager",
            LanguageId = 2,
            AvatarPath = "users/avatar.webp",
            BirthDate = new DateOnly(1995, 5, 20),
            GenderId = 1,
            MaritalStatusId = 1
        };

        mapper.Map(personalInfo, user);

        Assert.Equal(userId, user.Id);
        Assert.Equal("account@baim.az", user.Email);
        Assert.Equal("Aykhan", user.Name);
        Assert.Equal(new DateTime(1995, 5, 20), user.BirthDate);
        Assert.Equal("users/avatar.webp", user.AvatarPath);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        return services.BuildServiceProvider();
    }
}
