using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Data.Constants;
using ATMS.Admin.Service.Validation.Onboarding;
using Moq;

namespace Admin.Services.Tests.Validators.Onboarding;

public sealed class SaveInvitationsValidatorTest : BaseValidatorTest
{
    [Fact]
    public async Task Validate_WhenEmailsAreAvailable_ChecksThemInOneRepositoryCall()
    {
        var userId = Guid.NewGuid();
        SetupOnboarding(userId);
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetEmailsInUseAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var validator = new SaveInvitationsValidator(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object);
        var command = CreateCommand("first@baim.az", "second@baim.az");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        OnboardingRepositoryMock.Verify(x => x.GetEmailsInUseAsync(
            It.Is<IReadOnlyCollection<string>>(emails => emails.Count == 2),
            userId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Validate_WhenEmailIsDuplicated_DoesNotQueryDatabase()
    {
        SetupOnboarding(Guid.NewGuid());
        var validator = new SaveInvitationsValidator(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object);
        var command = CreateCommand("same@baim.az", " SAME@baim.az ");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        OnboardingRepositoryMock.Verify(x => x.GetEmailsInUseAsync(
            It.IsAny<IReadOnlyCollection<string>>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Validate_WhenEmailIsAlreadyUsed_ReturnsValidationFailure()
    {
        var userId = Guid.NewGuid();
        SetupOnboarding(userId);
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetEmailsInUseAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(["USED@BAIM.AZ"]);
        var validator = new SaveInvitationsValidator(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object);

        var result = await validator.ValidateAsync(CreateCommand("used@baim.az"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(SaveInvitationsCommand.Users));
    }

    private static SaveInvitationsCommand CreateCommand(params string[] emails)
    {
        return new SaveInvitationsCommand
        {
            Version = 0,
            Users = emails.Select((email, index) => new InvitedUserCommand
            {
                Name = $"Name{index}",
                Surname = $"Surname{index}",
                Email = email
            }).ToList()
        };
    }

    private void SetupOnboarding(Guid userId)
    {
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        CurrentUserMock.SetupGet(x => x.RoleId).Returns(RoleIds.ClientManager);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OnboardingProgress
            {
                Version = 0,
                User = new User()
            });
    }
}
