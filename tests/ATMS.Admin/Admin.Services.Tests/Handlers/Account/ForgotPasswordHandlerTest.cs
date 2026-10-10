using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class ForgotPasswordHandlerTest : BaseHandlerTest
{
    private readonly ForgotPasswordHandler _handler;
 
    public ForgotPasswordHandlerTest()
    {
        _handler = new ForgotPasswordHandler(
            UserRepositoryMock.Object,
            EmailDeliveryRepositoryMock.Object,
            new EmailDeliveryRequestLock());
        EmailDeliveryRepositoryMock
            .Setup(x => x.AnySinceAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }
 
    private ForgotPasswordCommand CreateCommand(string? email = null) =>
        new() { Email = email ?? Faker.Internet.Email() };
 
    [Fact]
    public async Task Handle_WhenUserExists_QueuesPasswordResetAndSaves()
    {
        var command = CreateCommand();
        var user = new User { Email = command.Email };
 
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        await _handler.Handle(command, CancellationToken.None);

        EmailDeliveryRepositoryMock.Verify(
            x => x.AnySinceAsync(
                user.Id,
                EmailDeliveryTypeEnum.PasswordReset,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        EmailDeliveryRepositoryMock.Verify(
            x => x.RemoveUnsentAsync(
                user.Id,
                EmailDeliveryTypeEnum.PasswordReset,
                It.IsAny<CancellationToken>()),
            Times.Once);
        EmailDeliveryRepositoryMock.Verify(
            x => x.AddPasswordResetAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
 
    // Same answer for an unknown address: the endpoint must not reveal who is registered.
    [Fact]
    public async Task Handle_WhenUserNotFound_SucceedsWithoutQueuingAnEmail()
    {
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await _handler.Handle(CreateCommand(), CancellationToken.None);

        UserRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        EmailDeliveryRepositoryMock.Verify(
            x => x.AnySinceAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Matching the stored address in any letter case: with one answer for every email, a mismatch
    // would fail silently and no letter would ever come.
    [Fact]
    public async Task Handle_EmailInDifferentCase_FindsUserByNormalizedEmail()
    {
        var user = new User { Email = "Leyla@Example.com", NormalizedEmail = "LEYLA@EXAMPLE.COM" };
        Expression<Func<User, bool>>? predicate = null;
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .Callback<Expression<Func<User, bool>>, CancellationToken>((filter, _) => predicate = filter)
            .ReturnsAsync(user);

        await _handler.Handle(CreateCommand(" leyla@example.com "), CancellationToken.None);

        Assert.NotNull(predicate);
        Assert.True(predicate.Compile()(user));
    }

    [Fact]
    public async Task Handle_WhenTheLastPasswordResetIsAMinuteOld_DoesNotQueueAnother()
    {
        var command = CreateCommand();
        var user = new User { Id = Guid.NewGuid(), Email = command.Email };
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var since = Recent(user.Id, EmailDeliveryTypeEnum.PasswordReset, DateTime.UtcNow.AddMinutes(-1));

        await _handler.Handle(command, CancellationToken.None);

        AssertPause(since);
        EmailDeliveryRepositoryMock.Verify(
            x => x.AddPasswordResetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        EmailDeliveryRepositoryMock.Verify(
            x => x.RemoveUnsentAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTheLastPasswordResetIsThreeMinutesOld_QueuesAnother()
    {
        var command = CreateCommand();
        var user = new User { Id = Guid.NewGuid(), Email = command.Email };
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var since = Recent(user.Id, EmailDeliveryTypeEnum.PasswordReset, DateTime.UtcNow.AddMinutes(-3));

        await _handler.Handle(command, CancellationToken.None);

        AssertPause(since);
        EmailDeliveryRepositoryMock.Verify(
            x => x.RemoveUnsentAsync(
                user.Id,
                EmailDeliveryTypeEnum.PasswordReset,
                It.IsAny<CancellationToken>()),
            Times.Once);
        EmailDeliveryRepositoryMock.Verify(
            x => x.AddPasswordResetAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // The handler asks "any letter of this type since now minus the pause"; the age decides the answer.
    private Func<DateTime?> Recent(Guid userId, EmailDeliveryTypeEnum type, DateTime createdAt)
    {
        DateTime? since = null;
        EmailDeliveryRepositoryMock
            .Setup(x => x.AnySinceAsync(
                It.IsAny<Guid>(),
                It.IsAny<EmailDeliveryTypeEnum>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns((Guid id, EmailDeliveryTypeEnum asked, DateTime from, CancellationToken _) =>
            {
                since = from;
                return Task.FromResult(id == userId && asked == type && createdAt >= from);
            });
        return () => since;
    }

    private static void AssertPause(Func<DateTime?> since)
    {
        var from = since();
        Assert.NotNull(from);
        Assert.InRange(
            from.Value,
            DateTime.UtcNow.AddMinutes(-2).AddSeconds(-3),
            DateTime.UtcNow.AddMinutes(-2).AddSeconds(1));
    }
}
