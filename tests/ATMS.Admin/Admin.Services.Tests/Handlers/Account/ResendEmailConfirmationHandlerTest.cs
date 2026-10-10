using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
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
        EmailDeliveryRepositoryMock
            .Setup(x => x.AnySinceAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
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
            x => x.AnySinceAsync(
                user.Id,
                EmailDeliveryTypeEnum.Confirmation,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
 
        Assert.Equal(EntityErrorTypeEnum.NotFound, exception.ErrorType);
        EmailDeliveryRepositoryMock.Verify(
            x => x.AnySinceAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
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

        Assert.Equal(AuthErrorTypeEnum.EmailAlreadyConfirmed, exception.AuthErrorType);
        EmailDeliveryRepositoryMock.Verify(
            x => x.AnySinceAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTheLastConfirmationIsAMinuteOld_LeavesThePasswordAndQueuesNothing()
    {
        var command = CreateCommand();
        var user = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = "old-hash" };
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        DateTime? since = null;
        var createdAt = DateTime.UtcNow.AddMinutes(-1);
        EmailDeliveryRepositoryMock
            .Setup(x => x.AnySinceAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns((Guid id, EmailDeliveryTypeEnum type, DateTime from, CancellationToken _) =>
            {
                since = from;
                return Task.FromResult(
                    id == user.Id && type == EmailDeliveryTypeEnum.Confirmation && createdAt >= from);
            });

        await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(since);
        Assert.InRange(
            since.Value,
            DateTime.UtcNow.AddMinutes(-2).AddSeconds(-3),
            DateTime.UtcNow.AddMinutes(-2).AddSeconds(1));
        Assert.Equal("old-hash", user.PasswordHash);
        PasswordServiceMock.Verify(p => p.GenerateRandomPassword(), Times.Never);
        EmailDeliveryRepositoryMock.Verify(
            x => x.AddConfirmationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        EmailDeliveryRepositoryMock.Verify(
            x => x.RemoveUnsentAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
