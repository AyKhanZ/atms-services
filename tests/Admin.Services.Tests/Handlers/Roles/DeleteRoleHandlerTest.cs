using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Service.Handlers.Roles;
using Moq;

namespace Admin.Services.Tests.Handlers.Roles;

public class DeleteRoleHandlerTest : BaseHandlerTest
{
    private readonly DeleteRoleHandler _handler;
 
    public DeleteRoleHandlerTest()
    {
        _handler = new DeleteRoleHandler(RoleRepositoryMock.Object);
    }
 
    [Fact]
    public async Task Handle_CallsDeleteAsyncWithCorrectId()
    {
        var command = new DeleteRoleCommand { Id = Guid.NewGuid() };
 
        await _handler.Handle(command, CancellationToken.None);
 
        RoleRepositoryMock.Verify(r => r.DeleteAsync(command.Id, 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
