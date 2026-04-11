using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Roles;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Roles;

public class DeleteRoleHandlerTest : BaseHandlerTest
{
    private readonly DeleteRoleHandler _handler;
 
    public DeleteRoleHandlerTest()
    {
        _handler = new DeleteRoleHandler(RoleRepositoryMock.Object);
    }
 
    private DeleteRoleCommand CreateCommand(Guid? id = null) =>
        new() { Id = id ?? Guid.NewGuid() };

    private void SetupRoleExists(bool exists) =>
        RoleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

    [Fact]
    public async Task Handle_WhenRoleExists_CallsDeleteAsyncWithCorrectId()
    {
        var command = CreateCommand();
        SetupRoleExists(true);

        await _handler.Handle(command, CancellationToken.None);

        RoleRepositoryMock.Verify(r => r.DeleteAsync(command.Id,
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
    public async Task Handle_WhenRoleNotFound_DoesNotCallDeleteAsync()
    {
        var command = CreateCommand();
        SetupRoleExists(false);

        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        RoleRepositoryMock.Verify(r => r.DeleteAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
