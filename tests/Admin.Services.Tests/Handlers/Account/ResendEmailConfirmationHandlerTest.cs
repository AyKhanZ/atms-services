using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Auth;
using ATMS.Email.Models;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class ResendEmailConfirmationHandlerTest : BaseHandlerTest
{
    private readonly ResendEmailConfirmationHandler _handler;
 
    private const string FakeToken = "fake-email-token";
    private const string FakePassword = "RandPass1!";
    private const string FakePasswordHash = "hashed-password";
 
    public ResendEmailConfirmationHandlerTest()
    {
        _handler = new ResendEmailConfirmationHandler(
            UserRepositoryMock.Object,
            PasswordHasherServiceMock.Object,
            PasswordServiceMock.Object,
            EmailConfirmationTokenServiceMock.Object,
            EmailSenderMock.Object,
            BuildConfiguration());
 
        PasswordServiceMock
            .Setup(p => p.GenerateRandomPassword())
            .Returns(FakePassword);
 
        PasswordHasherServiceMock
            .Setup(p => p.Hash(FakePassword))
            .Returns(FakePasswordHash);
 
        EmailConfirmationTokenServiceMock
            .Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns(new EmailConfirmationTokenResult(FakeToken, DateTime.UtcNow.AddHours(24)));
    }
 
    private ResendEmailConfirmationCommand CreateCommand(string? email = null) =>
        new() { Email = email ?? Faker.Internet.Email() };
 
    [Fact]
    public async Task Handle_WhenUserExists_SendsEmailWithCorrectLink()
    {
        var command = CreateCommand();
        var user = new User { Email = command.Email, Name = "John", Surname = "Doe" };
 
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        await _handler.Handle(command, CancellationToken.None);
 
        EmailSenderMock.Verify(s => s.SendAsync(
            user.Email,
            It.Is<InviteModel>(m => m.Link.Contains(FakeToken) && m.Link.Contains(BaseUrl)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task Handle_WhenUserExists_UpdatesPasswordHash()
    {
        var command = CreateCommand();
        var user = new User { Email = command.Email };
 
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        await _handler.Handle(command, CancellationToken.None);
 
        Assert.Equal(FakePasswordHash, user.PasswordHash);
    }
 
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
 
        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(CreateCommand(), CancellationToken.None));
 
        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
    
    [Fact]
    public async Task Handle_WhenUserEmailAlreadyConfirmed_ThrowsAuthException()
    {
        var command = CreateCommand();
        var user = new User { Email = command.Email, EmailConfirmed = true };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorType.EmailAlreadyConfirmed, exception.AuthErrorType);
    }
}
