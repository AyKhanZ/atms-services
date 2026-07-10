using ATMS.Admin.Contracts.Models.UserProgresses;
using ATMS.Admin.Contracts.Requests.UserProgresses;
using ATMS.Admin.Data.Entities.UserProgresses;
using ATMS.Admin.Service.Handlers.UserProgresses;
using ATMS.Data.Enums;
using Moq;

namespace Admin.Services.Tests.Handlers.UserProgresses;

public class GetUserProgressHandlerTest : BaseHandlerTest
{
    private readonly GetUserProgressHandler _handler;

    public GetUserProgressHandlerTest()
    {
        _handler = new GetUserProgressHandler(
            CurrentUserMock.Object,
            UserProgressRepositoryMock.Object,
            MapperMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProgressNotFound_ReturnsEmptyModelWithUserType()
    {
        // Arrange
        var userType = "Client";

        CurrentUserMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        CurrentUserMock.Setup(x => x.UserType).Returns(userType);

        UserProgressRepositoryMock
            .Setup(x => x.GetAsync(CurrentUserMock.Object.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProgress?)null);

        // Act
        var result = await _handler.Handle(new GetUserProgressRequest(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userType, result.UserProgressType);
        MapperMock.Verify(x => x.Map<UserProgressModel>(It.IsAny<UserProgress>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProgressFound_ReturnsMappedModel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var progress = new UserProgress
        {
            UserId = userId,
            UserProgressType = UserProgressTypeEnum.Client,
            CurrentStep = 2,
            LastUpdated = DateTime.UtcNow
        };

        var expectedModel = new UserProgressModel
        {
            UserProgressType = "Client",
            CurrentStep = 2
        };

        CurrentUserMock.Setup(x => x.Id).Returns(userId);

        UserProgressRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        MapperMock
            .Setup(x => x.Map<UserProgressModel>(progress))
            .Returns(expectedModel);

        // Act
        var result = await _handler.Handle(new GetUserProgressRequest(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedModel.UserProgressType, result.UserProgressType);
        Assert.Equal(expectedModel.CurrentStep, result.CurrentStep);
        MapperMock.Verify(x => x.Map<UserProgressModel>(progress), Times.Once);
    }
}