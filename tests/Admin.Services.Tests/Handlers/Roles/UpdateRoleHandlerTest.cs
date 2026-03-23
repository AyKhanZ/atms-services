using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Roles;
using Moq;

namespace Admin.Services.Tests.Handlers.Roles;

public class UpdateRoleHandlerTest : BaseHandlerTest
{
    private readonly UpdateRoleHandler _handler;
 
    public UpdateRoleHandlerTest()
    {
        _handler = new UpdateRoleHandler(RoleRepositoryMock.Object, MapperMock.Object);
    }
 
    [Fact]
    public async Task Handle_CallsUpdateAsyncWithMappedEntity()
    {
        var command = new UpdateRoleCommand { Id = Guid.NewGuid(), Name = "Admin" };
        var entity = new Role { Id = command.Id, Name = command.Name };
 
        MapperMock.Setup(m => m.Map<Role>(command)).Returns(entity);
 
        await _handler.Handle(command, CancellationToken.None);
 
        RoleRepositoryMock.Verify(r => r.UpdateAsync(entity,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
