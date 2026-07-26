using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Onboarding;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using Moq;

namespace Admin.Services.Tests.Validators.Onboarding;

public sealed class CompleteOnboardingValidatorTest : BaseValidatorTest
{
    [Fact]
    public async Task Validate_WhenSecurityIsIncomplete_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        CurrentUserMock.SetupGet(x => x.RoleId).Returns(RoleIds.ClientManager);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsNoTrackingAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OnboardingProgress
            {
                Version = 7,
                PersonalInfoStatus = OnboardingStepStatusEnum.Completed,
                PersonalInfo = new OnboardingPersonalInfo(),
                SecurityStatus = OnboardingStepStatusEnum.NotStarted,
                User = new User()
            });
        var validator = new CompleteOnboardingValidator(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            validator.ValidateAsync(new CompleteOnboardingCommand { Version = 7 }));

        Assert.Equal(OnboardingMessages.SecurityIncomplete, exception.Message);
    }
}
