using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Roles;
using ATMS.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Roles;

public class UpdateRoleHandlerTest : BaseHandlerTest
{
    private readonly UpdateRoleHandler _handler;
 
    public UpdateRoleHandlerTest()
    {
        _handler = new UpdateRoleHandler(RoleRepositoryMock.Object, MapperMock.Object);
    }
 
    private UpdateRoleCommand CreateCommand(Guid? id = null) =>
        new() { Id = id ?? Guid.NewGuid(), Name = "Admin" };

    private void SetupRoleExists(bool exists) =>
        RoleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

    [Fact]
    public async Task Handle_WhenRoleExists_CallsUpdateAsyncWithMappedEntity()
    {
        var command = CreateCommand();
        var entity = new Role { Id = command.Id, Name = command.Name };

        SetupRoleExists(true);
        MapperMock.Setup(m => m.Map<Role>(command)).Returns(entity);

        await _handler.Handle(command, CancellationToken.None);

        RoleRepositoryMock.Verify(r => r.UpdateAsync(entity,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ThrowsEntityException()
    {
        var command = CreateCommand();
        SetupRoleExists(false);

        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_DoesNotCallUpdateAsync()
    {
        var command = CreateCommand();
        SetupRoleExists(false);

        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        RoleRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<Role>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_DoesNotCallMapper()
    {
        var command = CreateCommand();
        SetupRoleExists(false);

        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        MapperMock.Verify(m => m.Map<Role>(It.IsAny<UpdateRoleCommand>()), Times.Never);
    }
}
