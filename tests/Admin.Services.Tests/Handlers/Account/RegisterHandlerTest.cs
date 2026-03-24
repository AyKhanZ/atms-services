using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Security.Models;
using ATMS.Email.Models;
using ATMS.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class RegisterHandlerTest : BaseHandlerTest
{
    private readonly RegisterHandler _handler;
 
    private const string FakePassword = "RandPass1!";
    private const string FakePasswordHash = "hashed-password";
    private const string FakeToken = "fake-email-token";
 
    public RegisterHandlerTest()
    {
        _handler = new RegisterHandler(
            UserRepositoryMock.Object,
            RoleRepositoryMock.Object,
            MapperMock.Object,
            PasswordServiceMock.Object,
            PasswordHasherServiceMock.Object,
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
 
 
    private RegisterCommand CreateCommand(Guid? roleId = null) =>
        new()
        {
            Email = Faker.Internet.Email(),
            Name = Faker.Name.FirstName(),
            Surname = Faker.Name.LastName(),
            RoleId = roleId ?? Guid.NewGuid()
        };
 
    private void SetupRole(Guid roleId) =>
        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = roleId, Name = "Admin" });
 
    [Fact]
    public async Task Handle_WhenRoleExists_ReturnsMappedUserModel()
    {
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid(), Email = command.Email, Name = command.Name, Surname = command.Surname };
        var expectedModel = new UserModel { Id = entity.Id };
 
        MapperMock.Setup(m => m.Map<User>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<UserModel>(entity)).Returns(expectedModel);
 
        SetupRole(command.RoleId);
 
        var result = await _handler.Handle(command, CancellationToken.None);
 
        Assert.Equal(expectedModel, result);
    }
 
    [Fact]
    public async Task Handle_WhenRoleExists_SetsHashedPassword()
    {
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid() };
 
        MapperMock.Setup(m => m.Map<User>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<UserModel>(entity)).Returns(new UserModel());
 
        SetupRole(command.RoleId);
 
        await _handler.Handle(command, CancellationToken.None);
 
        Assert.Equal(FakePasswordHash, entity.PasswordHash);
    }
 
    [Fact]
    public async Task Handle_WhenRoleExists_SendsEmailWithCorrectLink()
    {
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid(), Email = command.Email, Name = command.Name, Surname = command.Surname };
 
        MapperMock.Setup(m => m.Map<User>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<UserModel>(entity)).Returns(new UserModel());
 
        SetupRole(command.RoleId);
 
        await _handler.Handle(command, CancellationToken.None);
 
        EmailSenderMock.Verify(s => s.SendAsync(
            entity.Email,
            It.Is<InviteModel>(m => m.Link.Contains(FakeToken) && m.Link.Contains(BaseUrl)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenRoleNotFound_ThrowsEntityException()
    {
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid() };

        MapperMock.Setup(m => m.Map<User>(command)).Returns(entity);

        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
        Assert.Equal("Role not found .", exception.Message);
    }
}
