using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Security.Models;
using ATMS.Email.Models;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class ForgotPasswordHandlerTest : BaseHandlerTest
{
    private readonly ForgotPasswordHandler _handler;
 
    private const string FakeToken = "fake-reset-token";
    public ForgotPasswordHandlerTest()
    {
 
        _handler = new ForgotPasswordHandler(
            UserRepositoryMock.Object,
            EmailSenderMock.Object,
            ResetPasswordTokenServiceMock.Object,
            BuildConfiguration());
 
        ResetPasswordTokenServiceMock
            .Setup(s => s.GenerateTokenAsync(It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResetPasswordTokenResult(FakeToken, DateTime.UtcNow.AddHours(2)));
    }
 
    private ForgotPasswordCommand CreateCommand(string? email = null) =>
        new() { Email = email ?? Faker.Internet.Email() };
 
    [Fact]
    public async Task Handle_WhenUserExists_SendsEmailWithCorrectLink()
    {
        var command = CreateCommand();
        var user = new User { Email = command.Email };
 
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        await _handler.Handle(command, CancellationToken.None);
 
        EmailSenderMock.Verify(s => s.SendAsync(
            user.Email,
            It.Is<ForgotPasswordModel>(m => m.Link.Contains(FakeToken) && m.Link.Contains(ResetPasswordPage)),
            It.IsAny<CancellationToken>()), Times.Once);
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
