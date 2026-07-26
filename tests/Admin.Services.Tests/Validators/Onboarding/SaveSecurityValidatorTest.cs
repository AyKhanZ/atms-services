using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Onboarding;
using ATMS.Application.Exceptions.Conflict;
using Moq;

namespace Admin.Services.Tests.Validators.Onboarding;

public sealed class SaveSecurityValidatorTest : BaseValidatorTest
{
    private readonly SaveSecurityValidator _validator;

    public SaveSecurityValidatorTest()
    {
        _validator = new SaveSecurityValidator(CurrentUserMock.Object, OnboardingRepositoryMock.Object);
    }

    [Fact]
    public async Task Validate_AcceptsStrongMatchingPasswords()
    {
        SetupOnboarding(version: 0);
        var result = await _validator.ValidateAsync(new SaveSecurityCommand
        {
            Password = "Baim@2026!",
            ConfirmPassword = "Baim@2026!",
            Version = 0
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("weak", "weak")]
    [InlineData("Baim@2026!", "Other@2026!")]
    public async Task Validate_RejectsWeakOrMismatchedPasswords(string password, string confirmation)
    {
        SetupOnboarding(version: 0);
        var result = await _validator.ValidateAsync(new SaveSecurityCommand
        {
            Password = password,
            ConfirmPassword = confirmation,
            Version = 0
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenVersionIsOutdated_ThrowsConflictException()
    {
        SetupOnboarding(version: 1);

        var exception = await Assert.ThrowsAsync<ConflictException>(() => _validator.ValidateAsync(new SaveSecurityCommand
        {
            Password = "Baim@2026!",
            ConfirmPassword = "Baim@2026!",
            Version = 0
        }));

        Assert.Equal(OnboardingMessages.OnboardingConcurrencyConflict, exception.Message);
    }

    private void SetupOnboarding(long version)
    {
        var userId = Guid.NewGuid();
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsNoTrackingAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OnboardingProgress
            {
                Version = version,
                User = new User()
            });
    }
}
