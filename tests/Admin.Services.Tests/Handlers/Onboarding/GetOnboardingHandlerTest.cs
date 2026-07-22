using ATMS.Admin.Contracts.Requests.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Handlers.Onboarding;
using ATMS.Data.Constants;
using Moq;

namespace Admin.Services.Tests.Handlers.Onboarding;

public sealed class GetOnboardingHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_ReturnsStableRoleCodeAndFirstIncompleteStep()
    {
        var userId = Guid.NewGuid();
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        var progress = CreateProgress(userId, RoleIds.ClientManager);
        var expected = new OnboardingModel
        {
            Role = "clientManager",
            CurrentStep = "personalInfo",
            Steps = [new(), new(), new()],
            InvitedUsers = [],
            PersonalInfo = new()
        };
        OnboardingRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        MapperMock.Setup(x => x.Map<OnboardingModel>(progress)).Returns(expected);
        var handler = new GetOnboardingHandler(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object,
            MapperMock.Object);

        var result = await handler.Handle(new GetOnboardingRequest(), CancellationToken.None);

        Assert.Equal("clientManager", result.Role);
        Assert.Equal("personalInfo", result.CurrentStep);
        Assert.Equal(3, result.Steps.Length);
        Assert.Empty(result.InvitedUsers);
    }

    private static OnboardingProgress CreateProgress(Guid userId, Guid roleId)
    {
        return new OnboardingProgress
        {
            UserId = userId,
            User = new User
            {
                Id = userId,
                Name = "Aykhan",
                Surname = "Zeynalov",
                Email = "aykhan@baim.az",
                NormalizedEmail = "AYKHAN@BAIM.AZ",
                AvatarPath = DefaultValues.UserAvatar,
                PasswordHash = "hash",
                LanguageId = 2,
                UserRoles = [new UserRole { UserId = userId, RoleId = roleId }]
            },
            UpdatedAt = DateTime.UtcNow
        };
    }
}
