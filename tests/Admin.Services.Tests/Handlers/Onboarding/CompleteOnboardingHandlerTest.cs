using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Handlers.Onboarding;
using ATMS.Admin.Service.Security.Models;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Messaging.Configuration;
using ATMS.Application.Exceptions.Conflict;
using Moq;

namespace Admin.Services.Tests.Handlers.Onboarding;

public sealed class CompleteOnboardingHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_SavesOnboardingAndPublishesUserEvents()
    {
        var userId = Guid.NewGuid();
        var progress = CreateReadyProgress(userId, Guid.NewGuid());
        UserUpdatedEvent? updatedEvent = null;
        UserInvitedEvent? invitedEvent = null;
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        OnboardingRepositoryMock
            .Setup(x => x.TrySaveAsync(progress, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        CacheServiceMock
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        AccessTokenServiceMock
            .Setup(x => x.GenerateTokenAsync(progress.User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult("new-access-token", DateTime.UtcNow.AddMinutes(15)));
        OutboxRepositoryMock
            .Setup(x => x.AddAsync(
                MessagingConstants.Exchanges.UserEvents,
                MessagingConstants.RoutingKeys.UserUpdated,
                It.IsAny<UserUpdatedEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, UserUpdatedEvent, CancellationToken>((_, _, message, _) => updatedEvent = message)
            .ReturnsAsync(Guid.NewGuid());
        OutboxRepositoryMock
            .Setup(x => x.AddAsync(
                MessagingConstants.Exchanges.UserEvents,
                MessagingConstants.RoutingKeys.UserInvited,
                It.IsAny<UserInvitedEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, UserInvitedEvent, CancellationToken>((_, _, message, _) => invitedEvent = message)
            .ReturnsAsync(Guid.NewGuid());
        var personalInfo = Assert.IsType<OnboardingPersonalInfo>(progress.PersonalInfo);
        MapperMock
            .Setup(x => x.Map<OnboardingPersonalInfo, User>(personalInfo, progress.User))
            .Callback<OnboardingPersonalInfo, User>((source, destination) =>
            {
                destination.Name = source.Name;
                destination.Surname = source.Surname;
                destination.PhoneNumber = source.PhoneNumber;
                destination.Position = source.Position;
                destination.LanguageId = source.LanguageId;
                destination.AvatarPath = source.AvatarPath;
                destination.BirthDate = source.BirthDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                destination.GenderId = source.GenderId;
                destination.MaritalStatusId = source.MaritalStatusId;
            })
            .Returns(progress.User);
        var handler = CreateHandler();

        var result = await handler.Handle(new CompleteOnboardingCommand { Version = 7 }, CancellationToken.None);

        Assert.True(progress.User.HasCompletedOnboarding);
        Assert.NotNull(progress.User.OnboardingCompletedAt);
        Assert.Equal(DateTimeKind.Utc, progress.User.BirthDate!.Value.Kind);
        Assert.Equal("new-password-hash", progress.User.PasswordHash);
        Assert.Null(progress.PendingPasswordHash);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal(1, result.InvitationsQueued);
        Assert.Equal(userId, updatedEvent!.Id);
        Assert.Equal("colleague@baim.az", invitedEvent!.Email);
        Assert.Equal(userId, invitedEvent.InvitedByUserId);
        OnboardingRepositoryMock.Verify(
            x => x.TrySaveAsync(progress, 7, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOnboardingIsAlreadyCompleted_ReturnsFreshTokenWithoutPublishingEvents()
    {
        var userId = Guid.NewGuid();
        var progress = CreateReadyProgress(userId, Guid.NewGuid());
        progress.User.HasCompletedOnboarding = true;
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        AccessTokenServiceMock
            .Setup(x => x.GenerateTokenAsync(progress.User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult("fresh-token", DateTime.UtcNow.AddMinutes(15)));
        CacheServiceMock
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(
            new CompleteOnboardingCommand { Version = 1 },
            CancellationToken.None);

        Assert.Equal("fresh-token", result.AccessToken);
        OnboardingRepositoryMock.Verify(
            x => x.TrySaveAsync(It.IsAny<OnboardingProgress>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
        OutboxRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserUpdatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private CompleteOnboardingHandler CreateHandler()
    {
        return new CompleteOnboardingHandler(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object,
            MapperMock.Object,
            AccessTokenServiceMock.Object,
            CacheServiceMock.Object,
            OutboxRepositoryMock.Object);
    }

    private static OnboardingProgress CreateReadyProgress(Guid userId, Guid invitationId)
    {
        var user = new User
        {
            Id = userId,
            Name = "Old",
            Surname = "Name",
            Email = "manager@baim.az",
            NormalizedEmail = "MANAGER@BAIM.AZ",
            AvatarPath = DefaultValues.UserAvatar,
            PasswordHash = "old-password-hash",
            LanguageId = 2,
            OrganizationId = Guid.NewGuid(),
            UserRoles = [new UserRole { UserId = userId, RoleId = RoleIds.ClientManager }]
        };

        return new OnboardingProgress
        {
            UserId = userId,
            User = user,
            Version = 7,
            PersonalInfoStatus = OnboardingStepStatusEnum.Completed,
            SecurityStatus = OnboardingStepStatusEnum.Completed,
            InvitationsStatus = OnboardingStepStatusEnum.Completed,
            PendingPasswordHash = "new-password-hash",
            UpdatedAt = DateTime.UtcNow,
            PersonalInfo = new OnboardingPersonalInfo
            {
                Id = userId,
                Email = user.Email,
                Name = "Aykhan",
                Surname = "Zeynalov",
                PhoneNumber = "+994501112233",
                Position = "Operations manager",
                LanguageId = 2,
                AvatarPath = "/images/avatar.webp",
                BirthDate = new DateOnly(1995, 5, 20),
                GenderId = 1,
                MaritalStatusId = 1
            },
            InvitedUsers =
            [
                new OnboardingInvitedUser
                {
                    Id = invitationId,
                    OnboardingUserId = userId,
                    Email = "colleague@baim.az",
                    NormalizedEmail = "COLLEAGUE@BAIM.AZ",
                    Name = "Test",
                    Surname = "Colleague"
                }
            ]
        };
    }
}
