using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Entities.UserProgresses;
using ATMS.Admin.Service.Handlers.UserProgresses;
using ATMS.Data.Enums;
using Moq;

namespace Admin.Services.Tests.Handlers.UserProgresses;

public class UpdateUserProgressHandlerTest : BaseHandlerTest
{
    private readonly UpdateUserProgressHandler _handler;

    public UpdateUserProgressHandlerTest()
    {
        _handler = new UpdateUserProgressHandler(
            CurrentUserMock.Object,
            UserProgressRepositoryMock.Object,
            PasswordHasherServiceMock.Object);
    }

    #region Creates new progress when not exists

    [Fact]
    public async Task Handle_WhenProgressNotExists_CreatesNewProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        CurrentUserMock.Setup(x => x.Id).Returns(userId);
        CurrentUserMock.Setup(x => x.RoleId).Returns(roleId);
        CurrentUserMock.Setup(x => x.OrganizationId).Returns(organizationId);
        CurrentUserMock.Setup(x => x.UserType).Returns("Client");

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProgress?)null);

        var command = new UpdateUserProgressCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserProgressRepositoryMock.Verify(x => x.CreateAsync(
            It.Is<UserProgress>(p =>
                p.UserId == userId &&
                p.RoleId == roleId &&
                p.OrganizationId == organizationId &&
                p.UserProgressType == UserProgressTypeEnum.Client),
            It.IsAny<CancellationToken>()), Times.Once);

        UserProgressRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Password

    [Fact]
    public async Task Handle_WhenPasswordProvided_HashesAndSetsPassword()
    {
        // Arrange
        var progress = BuildProgress(UserProgressTypeEnum.Client);
        var password = "NewPassword1!";
        var hashedPassword = "hashed_password";

        SetupCurrentUser(progress.UserId, "Client");

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        PasswordHasherServiceMock
            .Setup(x => x.Hash(password))
            .Returns(hashedPassword);

        var command = new UpdateUserProgressCommand { Password = password };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(hashedPassword, progress.PasswordHash);
        PasswordHasherServiceMock.Verify(x => x.Hash(password), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsNull_DoesNotHashPassword()
    {
        // Arrange
        var progress = BuildProgress(UserProgressTypeEnum.Client);

        SetupCurrentUser(progress.UserId, "Client");

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        var command = new UpdateUserProgressCommand { Password = null };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        PasswordHasherServiceMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region UpdateCurrentStep

    [Theory]
    [InlineData(UserProgressTypeEnum.Client, false, false, 0)]
    [InlineData(UserProgressTypeEnum.Client, true, false, 1)]
    [InlineData(UserProgressTypeEnum.Employee, false, true, 1)]
    [InlineData(UserProgressTypeEnum.Client, true, true, 2)]
    [InlineData(UserProgressTypeEnum.Employee, true, true, 2)]
    public async Task Handle_UpdatesCurrentStep_ForClientAndEmployee(
        UserProgressTypeEnum progressType,
        bool hasPersonalInfo,
        bool hasPassword,
        ushort expectedStep)
    {
        // Arrange
        var progress = BuildProgress(progressType);
        if (hasPersonalInfo) progress.PersonalInfo = new PersonalInfo { Name = "John" };
        if (hasPassword) progress.PasswordHash = "hash";

        SetupCurrentUser(progress.UserId, progressType.ToString());

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        var command = new UpdateUserProgressCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedStep, progress.CurrentStep);
    }

    [Theory]
    [InlineData(false, false, false, 0)]
    [InlineData(true, false, false, 1)]
    [InlineData(false, true, false, 1)]
    [InlineData(false, false, true, 1)]
    [InlineData(true, true, false, 2)]
    [InlineData(true, false, true, 2)]
    [InlineData(false, true, true, 2)]
    [InlineData(true, true, true, 3)]
    public async Task Handle_UpdatesCurrentStep_ForClientManager(
        bool hasPersonalInfo,
        bool hasPassword,
        bool hasInvitedUsers,
        ushort expectedStep)
    {
        // Arrange
        var progress = BuildProgress(UserProgressTypeEnum.ClientManager);
        if (hasPersonalInfo) progress.PersonalInfo = new PersonalInfo { Name = "John" };
        if (hasPassword) progress.PasswordHash = "hash";
        if (hasInvitedUsers) progress.InvitedUsers = [new InvitedUser { Email = "test@test.com" }];

        SetupCurrentUser(progress.UserId, "ClientManager");

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        var command = new UpdateUserProgressCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedStep, progress.CurrentStep);
    }

    #endregion

    #region UpdatePersonalInfo

    [Fact]
    public async Task Handle_WhenPersonalInfoProvided_UpdatesPersonalInfo()
    {
        // Arrange
        var progress = BuildProgress(UserProgressTypeEnum.Client);

        SetupCurrentUser(progress.UserId, "Client");

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        var command = new UpdateUserProgressCommand
        {
            PersonalInfoCommand = new PersonalInfoCommand
            {
                Name = Faker.Name.FirstName(),
                Surname = Faker.Name.LastName(),
                Email = Faker.Internet.Email(),
                PhoneNumber = "+994501234567",
                Position = "Developer",
                Language = "en",
                AvatarPath = "avatar.png",
                BirthDate = new DateTime(1995, 1, 1),
                GenderId = 1,
                MaritalStatusId = 1
            }
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(progress.PersonalInfo);
        Assert.Equal(command.PersonalInfoCommand.Name, progress.PersonalInfo.Name);
        Assert.Equal(command.PersonalInfoCommand.Email, progress.PersonalInfo.Email);
        Assert.Equal(command.PersonalInfoCommand.Surname, progress.PersonalInfo.Surname);
    }

    [Fact]
    public async Task Handle_WhenPersonalInfoIsNull_DoesNotUpdatePersonalInfo()
    {
        // Arrange
        var progress = BuildProgress(UserProgressTypeEnum.Client);

        SetupCurrentUser(progress.UserId, "Client");

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        var command = new UpdateUserProgressCommand { PersonalInfoCommand = null };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(progress.PersonalInfo);
    }

    #endregion

    #region UpdateInvitedUsers

    [Fact]
    public async Task Handle_WhenInvitedUsersProvided_ReplacesInvitedUsers()
    {
        // Arrange
        var progress = BuildProgress(UserProgressTypeEnum.ClientManager);
        progress.InvitedUsers = [new InvitedUser { Email = "old@test.com" }];

        SetupCurrentUser(progress.UserId, "ClientManager");

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        var command = new UpdateUserProgressCommand
        {
            InvitedUsersCommand =
            [
                new InvitedUsersCommand { Name = "John", Surname = "Doe", Email = "new@test.com" }
            ]
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(progress.InvitedUsers!);
        Assert.Equal("new@test.com", progress.InvitedUsers![0].Email);
    }

    [Fact]
    public async Task Handle_WhenInvitedUsersIsNull_DoesNotChangeInvitedUsers()
    {
        // Arrange
        var progress = BuildProgress(UserProgressTypeEnum.ClientManager);
        var existingUser = new InvitedUser { Email = "existing@test.com" };
        progress.InvitedUsers = [existingUser];

        SetupCurrentUser(progress.UserId, "ClientManager");

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        var command = new UpdateUserProgressCommand { InvitedUsersCommand = null };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(progress.InvitedUsers!);
        Assert.Equal("existing@test.com", progress.InvitedUsers![0].Email);
    }

    #endregion

    #region Helpers

    private void SetupCurrentUser(Guid userId, string userType)
    {
        CurrentUserMock.Setup(x => x.Id).Returns(userId);
        CurrentUserMock.Setup(x => x.UserType).Returns(userType);
    }

    private static UserProgress BuildProgress(UserProgressTypeEnum type) => new()
    {
        UserId = Guid.NewGuid(),
        UserProgressType = type,
        LastUpdated = DateTime.UtcNow
    };

    #endregion
}
