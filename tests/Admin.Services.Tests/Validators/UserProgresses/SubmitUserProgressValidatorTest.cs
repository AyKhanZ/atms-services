using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.UserProgresses;
using ATMS.Admin.Service.Validation.UserProgresses;
using ATMS.Application.Exceptions.Auth;
using ATMS.Data.Enums;
using Moq;

namespace Admin.Services.Tests.Validators.UserProgresses;

public class SubmitUserProgressValidatorTest : BaseValidatorTest
{
    private SubmitUserProgressValidator CreateValidator()
    {
        return new SubmitUserProgressValidator(
            UserProgressRepositoryMock.Object,
            UserRepositoryMock.Object,
            CurrentUserMock.Object);
    }

    #region Auth exceptions

    [Fact]
    public async Task Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();

        CurrentUserMock.Setup(x => x.Id).Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var validator = CreateValidator();

        var cmd = new SubmitUserProgressCommand();

        // Act & Assert
        await Assert.ThrowsAsync<AuthException>(() =>
            validator.ValidateAsync(cmd));
    }

    [Fact]
    public async Task Should_Throw_When_User_Already_Completed()
    {
        var userId = Guid.NewGuid();

        CurrentUserMock.Setup(x => x.Id).Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                HasCompletedSurvey = true
            });

        var validator = CreateValidator();

        var cmd = new SubmitUserProgressCommand();

        await Assert.ThrowsAsync<AuthException>(() =>
            validator.ValidateAsync(cmd));
    }

    [Fact]
    public async Task Should_Throw_When_Progress_Not_Found()
    {
        var userId = Guid.NewGuid();

        CurrentUserMock.Setup(x => x.Id).Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProgress?)null);

        var validator = CreateValidator();

        var cmd = new SubmitUserProgressCommand();

        await Assert.ThrowsAsync<AuthException>(() =>
            validator.ValidateAsync(cmd));
    }

    #endregion

    #region Validation logic

    [Theory]
    [InlineData(UserProgressTypeEnum.Client, 2, true)]
    [InlineData(UserProgressTypeEnum.Employee, 2, true)]
    [InlineData(UserProgressTypeEnum.ClientManager, 3, true)]
    [InlineData(UserProgressTypeEnum.ClientManager, 2, false)]
    [InlineData(UserProgressTypeEnum.Client, 1, false)]
    public async Task Should_Validate_CurrentStep_Correctly(
        UserProgressTypeEnum type,
        ushort currentStep,
        bool isValid)
    {
        // Arrange
        var userId = Guid.NewGuid();

        CurrentUserMock.Setup(x => x.Id).Returns(userId);

        UserRepositoryMock
            .Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress
            {
                UserId = userId,
                UserProgressType = type,
                CurrentStep = currentStep
            });

        var validator = CreateValidator();

        var cmd = new SubmitUserProgressCommand();

        // Act
        var result = await validator.ValidateAsync(cmd);

        // Assert
        Assert.Equal(isValid, result.IsValid);
    }

    #endregion
}
