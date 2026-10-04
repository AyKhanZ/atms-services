using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Modules;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Services.Tests.Mappers;

public sealed class ProfileModelMapperTest
{
    [Fact]
    public void Map_IncompleteUser_PreservesMissingFieldsAsNull()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var user = new User
        {
            Name = "Jane",
            Surname = "Doe",
            Email = "jane@example.com",
            AvatarPath = "default-avatar.png"
        };

        var model = mapper.Map<ProfileModel>(user);

        Assert.Equal("Jane", model.Name);
        Assert.Null(model.PhoneNumber);
        Assert.Null(model.Position);
        Assert.Null(model.BirthDate);
        Assert.Null(model.GenderId);
        Assert.Null(model.MaritalStatusId);
    }

    [Fact]
    public void Map_CompleteUser_ConvertsBirthDateAndDictionaryIds()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var user = new User
        {
            BirthDate = new DateTime(1990, 5, 20),
            GenderId = 2,
            MaritalStatusId = 1
        };

        var model = mapper.Map<ProfileModel>(user);

        Assert.Equal(new DateOnly(1990, 5, 20), model.BirthDate);
        Assert.Equal(2, model.GenderId);
        Assert.Equal(1, model.MaritalStatusId);
    }
}
