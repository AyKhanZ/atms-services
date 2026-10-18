using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Modules;
using ATMS.Data.Constants;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Services.Tests.Mappers;

public sealed class MeModelMapperTest
{
    public static TheoryData<Guid, string?> RoleLanguages => new()
    {
        { RoleIds.Employee, "AZ" },
        { RoleIds.SuperAdmin, null }
    };

    [Theory]
    [MemberData(nameof(RoleLanguages))]
    public void Map_User_SendsProfileLanguageExceptForSuperAdmin(Guid roleId, string? language)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var user = new User
        {
            Language = new Language { Code = "AZ" },
            UserRoles = [new UserRole { RoleId = roleId }]
        };

        var model = mapper.Map<MeModel>(user);

        Assert.Equal(language, model.Language);
    }
}
