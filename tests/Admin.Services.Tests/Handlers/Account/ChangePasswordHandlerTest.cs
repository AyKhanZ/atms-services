using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class ChangePasswordHandlerTest : BaseHandlerTest
{
    private readonly ChangePasswordHandler _handler;
 
    private const string NewPasswordHash = "hashed-new-password";
 
    public ChangePasswordHandlerTest()
    {
        _handler = new ChangePasswordHandler(UserRepositoryMock.Object, PasswordHasherServiceMock.Object);
 
        PasswordHasherServiceMock
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns(NewPasswordHash);
    }
 
    private ChangePasswordCommand CreateCommand(string? email = null, string? newPassword = null) =>
        new()
        {
            Email = email ?? "user@example.com",
            OldPassword = "OldPass1!",
            NewPassword = newPassword ?? "NewPass1!"
        };
 
    [Fact]
    public async Task Handle_WhenUserExists_UpdatesPasswordHash()
    {
        var user = new User { Email = "user@example.com", PasswordHash = "old-hash" };
        var command = CreateCommand();
 
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        await _handler.Handle(command, CancellationToken.None);
 
        Assert.Equal(NewPasswordHash, user.PasswordHash);
        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
 
        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(CreateCommand(), CancellationToken.None));
 
        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}
