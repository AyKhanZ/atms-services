using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Handlers.Onboarding;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Data.Enums;
using Moq;

namespace Admin.Services.Tests.Handlers.Onboarding;

public sealed class SaveInvitationsHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_ReplacesInvitationsAndCompletesStep()
    {
        var userId = Guid.NewGuid();
        var progress = CreateProgress(userId);
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        OnboardingRepositoryMock
            .Setup(x => x.TrySaveAsync(progress, 14, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        MapperMock
            .Setup(x => x.Map<OnboardingInvitedUser>(It.IsAny<InvitedUserCommand>()))
            .Returns<InvitedUserCommand>(command => new OnboardingInvitedUser
            {
                Name = command.Name,
                Surname = command.Surname,
                Email = command.Email
            });
        MapperMock
            .Setup(x => x.Map<OnboardingModel>(progress))
            .Returns(new OnboardingModel { Version = 15 });
        var handler = new SaveInvitationsHandler(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object,
            MapperMock.Object);

        var result = await handler.Handle(new SaveInvitationsCommand
        {
            Version = 14,
            Users =
            [
                new InvitedUserCommand
                {
                    Name = "Diane",
                    Surname = "Zeynalova",
                    Email = "diane@baim.az"
                }
            ]
        }, CancellationToken.None);

        var invitation = Assert.Single(progress.InvitedUsers);
        Assert.Equal(userId, invitation.OnboardingUserId);
        Assert.Equal("DIANE@BAIM.AZ", invitation.NormalizedEmail);
        Assert.Equal(OnboardingStepStatusEnum.Completed, progress.InvitationsStatus);
        Assert.Equal(15, result.Version);
        OnboardingRepositoryMock.Verify(
            x => x.TrySaveAsync(progress, 14, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVersionChanged_ThrowsConcurrencyConflict()
    {
        var userId = Guid.NewGuid();
        var progress = CreateProgress(userId);
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        OnboardingRepositoryMock
            .Setup(x => x.TrySaveAsync(progress, 14, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        MapperMock
            .Setup(x => x.Map<OnboardingInvitedUser>(It.IsAny<InvitedUserCommand>()))
            .Returns<InvitedUserCommand>(command => new OnboardingInvitedUser
            {
                Name = command.Name,
                Surname = command.Surname,
                Email = command.Email
            });
        var handler = new SaveInvitationsHandler(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object,
            MapperMock.Object);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new SaveInvitationsCommand
            {
                Version = 14,
                Users =
                [
                    new InvitedUserCommand
                    {
                        Name = "Diane",
                        Surname = "Zeynalova",
                        Email = "diane@baim.az"
                    }
                ]
            }, CancellationToken.None));

        Assert.Equal(OnboardingMessages.OnboardingConcurrencyConflict, exception.Message);
    }

    private static OnboardingProgress CreateProgress(Guid userId)
    {
        return new OnboardingProgress
        {
            UserId = userId,
            User = new User(),
            Version = 14,
            InvitationsStatus = OnboardingStepStatusEnum.Skipped,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
