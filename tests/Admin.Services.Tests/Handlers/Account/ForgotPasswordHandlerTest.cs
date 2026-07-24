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
}
