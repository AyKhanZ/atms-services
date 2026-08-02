using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Handlers.Onboarding;
using ATMS.Admin.Service.Resources;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Application.Exceptions.Conflict;
using Moq;

namespace Admin.Services.Tests.Handlers.Onboarding;

public sealed class SaveSecurityHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_StoresOnlyHashAndCompletesSecurityStep()
    {
        var userId = Guid.NewGuid();
        var progress = CreateProgress(userId);
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        OnboardingRepositoryMock
            .Setup(x => x.TrySaveAsync(progress, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        PasswordHasherServiceMock.Setup(x => x.Hash("Baim@2026!")).Returns("password-hash");
        MapperMock.Setup(x => x.Map<OnboardingModel>(progress))
            .Returns(new OnboardingModel { SecurityCompleted = true });
        var handler = new SaveSecurityHandler(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object,
            PasswordHasherServiceMock.Object,
            MapperMock.Object);

        var result = await handler.Handle(new SaveSecurityCommand
        {
            Password = "Baim@2026!",
            ConfirmPassword = "Baim@2026!",
            Version = 3
        }, CancellationToken.None);

        Assert.Equal("password-hash", progress.PendingPasswordHash);
        Assert.Equal(OnboardingStepStatusEnum.Completed, progress.SecurityStatus);
        Assert.True(result.SecurityCompleted);
        OnboardingRepositoryMock.Verify(x =>
            x.TrySaveAsync(progress, 3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProgressWasChanged_ThrowsLocalizedConcurrencyMessage()
    {
        var userId = Guid.NewGuid();
        var progress = CreateProgress(userId);
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        OnboardingRepositoryMock
            .Setup(x => x.TrySaveAsync(progress, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        PasswordHasherServiceMock.Setup(x => x.Hash("Baim@2026!")).Returns("password-hash");
        var handler = new SaveSecurityHandler(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object,
            PasswordHasherServiceMock.Object,
            MapperMock.Object);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new SaveSecurityCommand
            {
                Password = "Baim@2026!",
                ConfirmPassword = "Baim@2026!",
                Version = 3
            }, CancellationToken.None));

        Assert.Equal(OnboardingMessages.OnboardingConcurrencyConflict, exception.Message);
    }

    private static OnboardingProgress CreateProgress(Guid userId)
    {
        return new OnboardingProgress
        {
            UserId = userId,
            Version = 3,
            UpdatedAt = DateTime.UtcNow,
            User = new User
            {
                Id = userId,
                Name = "Aykhan",
                Surname = "Zeynalov",
                Email = "aykhan@baim.az",
                NormalizedEmail = "AYKHAN@BAIM.AZ",
                AvatarPath = DefaultValues.UserAvatar,
                PasswordHash = "old-hash",
                LanguageId = 2,
                UserRoles = [new UserRole { UserId = userId, RoleId = RoleIds.Client }]
            }
        };
    }
}
