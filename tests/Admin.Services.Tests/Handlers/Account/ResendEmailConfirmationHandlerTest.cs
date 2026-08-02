using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class ResendEmailConfirmationHandlerTest : BaseHandlerTest
{
    private readonly ResendEmailConfirmationHandler _handler;
 
    private const string FakePassword = "RandPass1!";
    private const string FakePasswordHash = "hashed-password";
 
    public ResendEmailConfirmationHandlerTest()
    {
        _handler = new ResendEmailConfirmationHandler(
            UserRepositoryMock.Object,
            PasswordHasherServiceMock.Object,
            PasswordServiceMock.Object,
            EmailDeliveryRepositoryMock.Object,
            new EmailDeliveryRequestLock());
 
        PasswordServiceMock
            .Setup(p => p.GenerateRandomPassword())
            .Returns(FakePassword);
 
        PasswordHasherServiceMock
            .Setup(p => p.Hash(FakePassword))
            .Returns(FakePasswordHash);
 
    }
 
    private ResendEmailConfirmationCommand CreateCommand(string? email = null) =>
        new() { Email = email ?? Faker.Internet.Email() };
 
    [Fact]
    public async Task Handle_WhenUserExists_QueuesConfirmationAndSaves()
    {
        var command = CreateCommand();
        var user = new User { Email = command.Email, Name = "John", Surname = "Doe" };
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        await _handler.Handle(command, CancellationToken.None);

        EmailDeliveryRepositoryMock.Verify(
            x => x.RemoveUnsentAsync(
                user.Id,
                EmailDeliveryTypeEnum.Confirmation,
                It.IsAny<CancellationToken>()),
            Times.Once);
        EmailDeliveryRepositoryMock.Verify(
            x => x.AddConfirmationAsync(user.Id, FakePassword, It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);
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
