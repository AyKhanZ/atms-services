using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Users;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Users;

public class GetUserHandlerTest : BaseHandlerTest
{
    private readonly GetUserHandler _handler;
    
    public GetUserHandlerTest()
    {
        _handler = new GetUserHandler(UserRepositoryMock.Object, MapperMock.Object);
    }
    
    private User CreateUser(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Email = Faker.Internet.Email(),
            Name = Faker.Name.FirstName(),
            Surname = Faker.Name.LastName()
        };
    

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsMappedModel()
    {
        var user = CreateUser();
        var expectedModel = new UserModel { Id = user.Id };
        var request = new GetUserRequest { Id = user.Id };

        UserRepositoryMock
            .Setup(r => r.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        MapperMock
            .Setup(m => m.Map<UserModel>(user))
            .Returns(expectedModel);
 
        var result = await _handler.Handle(request, CancellationToken.None);
 
        Assert.Equal(expectedModel, result);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        var request = new GetUserRequest { Id = Guid.NewGuid() };

        UserRepositoryMock
            .Setup(r => r.GetAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
 
        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(request, CancellationToken.None));
 
        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}
