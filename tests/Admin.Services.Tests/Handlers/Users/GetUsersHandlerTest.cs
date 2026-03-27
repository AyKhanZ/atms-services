using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Users;
using Moq;

namespace Admin.Services.Tests.Handlers.Users;

public class GetUsersHandlerTest : BaseHandlerTest
{
    private readonly GetUsersHandler _handler;
 
    public GetUsersHandlerTest()
    {
        _handler = new GetUsersHandler(UserRepositoryMock.Object, MapperMock.Object);
    }
 
    [Fact]
    public async Task Handle_ReturnsMappedUsers()
    {
        var users = new List<User> { new(), new() };
        var expectedModels = new UserModel[] { new(), new() };
 
        UserRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
 
        MapperMock
            .Setup(m => m.Map<UserModel[]>(users))
            .Returns(expectedModels);
 
        var result = await _handler.Handle(new GetUsersRequest(), CancellationToken.None);
 
        Assert.Equal(expectedModels, result);
    }
 
    [Fact]
    public async Task Handle_WhenNoUsers_ReturnsEmptyArray()
    {
        UserRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
 
        MapperMock
            .Setup(m => m.Map<UserModel[]>(It.IsAny<User[]>()))
            .Returns([]);
 
        var result = await _handler.Handle(new GetUsersRequest(), CancellationToken.None);
 
        Assert.Empty(result);
    }
}
