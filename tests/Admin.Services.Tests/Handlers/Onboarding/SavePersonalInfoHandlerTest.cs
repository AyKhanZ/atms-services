using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Handlers.Onboarding;
using ATMS.Data.Constants;
using ATMS.Infrastructure.Images;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Admin.Services.Tests.Handlers.Onboarding;

public sealed class SavePersonalInfoHandlerTest : BaseHandlerTest
{
    private readonly Mock<IImageStorage> _imageStorage = new();

    [Theory]
    [InlineData(DefaultValues.UserAvatar)]
    [InlineData("")]
    public async Task Handle_ReplacingSharedOrEmptyAvatar_DoesNotDeleteOldPath(string oldAvatarPath)
    {
        var userId = Guid.NewGuid();
        var avatar = new Mock<IFormFile>().Object;
        var progress = new OnboardingProgress
        {
            UserId = userId,
            User = new User { Email = "user@example.com" },
            PersonalInfo = new OnboardingPersonalInfo { AvatarPath = oldAvatarPath }
        };
        CurrentUserMock.SetupGet(x => x.Id).Returns(userId);
        OnboardingRepositoryMock.Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        OnboardingRepositoryMock.Setup(x => x.TrySaveAsync(progress, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _imageStorage.Setup(x => x.SaveAsync(avatar, ImageStorageFolder.Users, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredImage("new.webp", "url", "image/webp", 10));
        var handler = new SavePersonalInfoHandler(
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object,
            _imageStorage.Object,
            MapperMock.Object);
        var command = new SavePersonalInfoCommand
        {
            Name = "Jane",
            Surname = "Doe",
            PhoneNumber = "+994501234567",
            Position = "Developer",
            Avatar = avatar
        };

        await handler.Handle(command, CancellationToken.None);

        _imageStorage.Verify(x => x.DeleteAsync(oldAvatarPath, It.IsAny<CancellationToken>()), Times.Never);
    }
}
