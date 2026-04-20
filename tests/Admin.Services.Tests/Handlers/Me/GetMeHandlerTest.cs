using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Me;
using ATMS.Application.Exceptions.Auth;
using Moq;

namespace Admin.Services.Tests.Handlers.Me;

public class GetMeHandlerTest : BaseHandlerTest
{
    private readonly GetMeHandler _handler;
 
    public GetMeHandlerTest()
    {
        _handler = new GetMeHandler(UserRepositoryMock.Object, CurrentUserMock.Object, MapperMock.Object);
    }
    
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsMappedModel()
    {
        // Arrange
        var request = new GetMeRequest();
        var userId = Guid.NewGuid();

        var user = new User { Id = userId };
        var expected = new MeModel();

        CurrentUserMock
            .Setup(c => c.Id)
            .Returns(userId);

        UserRepositoryMock
            .Setup(r => r.GetMeAsync(userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MapperMock
            .Setup(m => m.Map<MeModel>(user))
            .Returns(expected);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsAuthException()
    {
        // Arrange
        var request = new GetMeRequest();
        var userId = Guid.NewGuid();

        CurrentUserMock
            .Setup(c => c.Id)
            .Returns(userId);

        UserRepositoryMock
            .Setup(r => r.GetMeAsync(userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(request, CancellationToken.None));

        // Assert
        Assert.Equal(AuthErrorType.InvalidCredentials, exception.AuthErrorType);
    }
    
    [Fact]
    public async Task Handle_Should_Use_CurrentUser_Id()
    {
        // Arrange
        var request = new GetMeRequest();
        var userId = Guid.NewGuid();

        CurrentUserMock
            .Setup(c => c.Id)
            .Returns(userId);

        UserRepositoryMock
            .Setup(r => r.GetMeAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        MapperMock
            .Setup(m => m.Map<MeModel>(It.IsAny<User>()))
            .Returns(new MeModel());

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(r =>
                r.GetMeAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_Call_Mapper_With_User()
    {
        // Arrange
        var request = new GetMeRequest();
        var userId = Guid.NewGuid();

        var user = new User { Id = userId };

        CurrentUserMock
            .Setup(c => c.Id)
            .Returns(userId);

        UserRepositoryMock
            .Setup(r => r.GetMeAsync(userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MapperMock
            .Setup(m => m.Map<MeModel>(user))
            .Returns(new MeModel());

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        MapperMock.Verify(m =>
                m.Map<MeModel>(user),
            Times.Once);
    }
}