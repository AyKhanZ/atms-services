using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Modules;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Services.Tests.Mappers;

public sealed class OnboardingModelMappingActionTest
{
    [Fact]
    public void Map_ClientManagerWithCompletedRequiredSteps_ReturnsInvitationsStep()
    {
        using var provider = BuildProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var progress = CreateProgress(RoleIds.ClientManager);
        progress.PersonalInfoStatus = OnboardingStepStatusEnum.Completed;
        progress.SecurityStatus = OnboardingStepStatusEnum.Completed;

        var model = mapper.Map<OnboardingModel>(progress);

        Assert.Equal("clientManager", model.Role);
        Assert.Equal("invitations", model.CurrentStep);
        Assert.Equal(3, model.Steps.Length);
        Assert.Equal(10, model.MaxInvitations);
    }

    [Fact]
    public void Map_UserWithAdditionalRole_UsesFirstRoleWithoutFailing()
    {
        using var provider = BuildProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var progress = CreateProgress(RoleIds.Client);
        progress.User.UserRoles.Add(new UserRole
        {
            UserId = progress.UserId,
            RoleId = RoleIds.Employee
        });

        var model = mapper.Map<OnboardingModel>(progress);

        Assert.Equal("client", model.Role);
    }

    private static OnboardingProgress CreateProgress(Guid roleId)
    {
        var userId = Guid.NewGuid();
        return new OnboardingProgress
        {
            UserId = userId,
            User = new User
            {
                Id = userId,
                Email = "account@baim.az",
                Name = "Aykhan",
                Surname = "Zeynalov",
                UserRoles =
                [
                    new UserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    }
                ]
            }
        };
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        return services.BuildServiceProvider();
    }
}
