using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Authentication;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Authentication;

public class RefreshTokenValidatorTest
{
    private readonly Faker _faker;
    private readonly RefreshTokenValidator _validator;
    private readonly Mock<IUserRepository> _userRepositoryMock;
 
    public RefreshTokenValidatorTest()
    {
        _faker = new Faker();
        _userRepositoryMock = new Mock<IUserRepository>();
        _validator = new RefreshTokenValidator(_userRepositoryMock.Object);
    }

    private RefreshTokenCommand GetCommand(string? refreshToken = null)
    {
        return new RefreshTokenCommand
        {
            RefreshToken = refreshToken ?? _faker.Random.AlphaNumeric(32)
        };
    }

    private void SetupFindUser(User? user)
    {
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private User CreateUserWithToken(DateTime? expiresAt = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = _faker.Random.AlphaNumeric(32),
            RefreshTokenExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7)
        };
    }
    
    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        SetupFindUser(CreateUserWithToken());
 
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.True(result.IsValid);
    }
 
    [Fact]
    public async Task Validate_WhenRefreshTokenIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(refreshToken: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Token is required .");
    }
 
    [Fact]
    public async Task Validate_WhenUserNotFound_ReturnsFailure()
    {
        SetupFindUser(null);
 
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "User with such refresh token doesn't exist .");
    }
 
    [Fact]
    public async Task Validate_WhenRefreshTokenExpired_ReturnsFailure()
    {
        SetupFindUser(CreateUserWithToken(expiresAt: DateTime.UtcNow.AddDays(-1)));
 
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Refresh token lifetime exceeded. Please log in again.");
    }
 
    [Fact]
    public async Task Validate_WhenRefreshTokenIsEmpty_DoesNotCheckExistence()
    {
        await _validator.ValidateAsync(GetCommand(refreshToken: string.Empty));
 
        _userRepositoryMock.Verify(
            r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
