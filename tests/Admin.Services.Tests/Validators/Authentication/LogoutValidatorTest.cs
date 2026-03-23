using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Authentication;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Authentication;

public class LogoutValidatorTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly LogoutValidator _validator;
    private readonly Faker _faker;
    private readonly Guid _userId = Guid.NewGuid();

    public LogoutValidatorTest()
    {
        _faker = new Faker();
        _userRepositoryMock = new Mock<IUserRepository>();
        _validator = new LogoutValidator(_userRepositoryMock.Object);
 
        SetupFindUser(new User());
    }

    private LogoutCommand GetCommand(string? refreshToken = null)
    {
        return new LogoutCommand
        {
            UserId =  _userId,
            RefreshToken = refreshToken ?? _faker.Random.AlphaNumeric(32)
        };
    }

    private void SetupFindUser(User? user)
    {
        user?.Id = _userId;
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        var command = GetCommand();
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.True(result.IsValid);
    }
 
    [Fact]
    public async Task Validate_WhenRefreshTokenIsEmpty_ReturnsFailure()
    {
        var command = GetCommand(refreshToken: string.Empty);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        _userRepositoryMock.Verify(
            r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "RefreshToken is required .");
    }
 
    [Fact]
    public async Task Validate_WhenRefreshTokenNotFound_ReturnsFailure()
    {
        SetupFindUser(null);
        var command = GetCommand();
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "User with such refresh token doesn't exist .");
    }
}
