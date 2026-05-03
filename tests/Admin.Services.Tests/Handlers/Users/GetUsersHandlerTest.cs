using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Handlers.Users;
using ATMS.Data.Criterias;
using Moq;

namespace Admin.Services.Tests.Handlers.Users;

public class GetUsersHandlerTest : BaseHandlerTest
{
    private readonly GetUsersHandler _handler;

    public GetUsersHandlerTest()
    {
        _handler = new GetUsersHandler(UserRepositoryMock.Object, MapperMock.Object);
    }

    private User CreateUser()
    {
        return new User
        {
            UserStatus = new UserStatus
            {
                Translations = new List<UserStatusTranslation>()
            }
        };
    }

    private PagedResult<User> CreatePagedResult(List<User> users) => new()
    {
        Items = users.ToArray(),
        TotalCount = users.Count,
        Page = 1,
        PageSize = 20
    };

    [Fact]
    public async Task Handle_ReturnsMappedPagedResult()
    {
        // Arrange
        var users = new List<User> { CreateUser(), CreateUser() };

        UserRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<ACriteria<User>>(),
                It.IsAny<PaginationCriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePagedResult(users));

        MapperMock
            .Setup(m => m.Map<UserListItemModel>(It.IsAny<User>()))
            .Returns(new UserListItemModel());

        // Act
        var result = await _handler.Handle(new GetUsersRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(users.Count, result.Items.Length);
        Assert.Equal(users.Count, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WhenNoUsers_ReturnsEmptyPagedResult()
    {
        // Arrange
        UserRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<ACriteria<User>>(),
                It.IsAny<PaginationCriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePagedResult([]));

        // Act
        var result = await _handler.Handle(new GetUsersRequest(), CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Handle_MapsEachUser()
    {
        // Arrange
        var users = new List<User> { CreateUser(), CreateUser(), CreateUser() };

        UserRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<ACriteria<User>>(),
                It.IsAny<PaginationCriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePagedResult(users));

        MapperMock
            .Setup(m => m.Map<UserListItemModel>(It.IsAny<User>()))
            .Returns(new UserListItemModel());

        // Act
        await _handler.Handle(new GetUsersRequest(), CancellationToken.None);

        // Assert
        MapperMock.Verify(m =>
                m.Map<UserListItemModel>(It.IsAny<User>()),
            Times.Exactly(users.Count));
    }

    [Fact]
    public async Task Handle_PagedResultMetadata_IsPreserved()
    {
        // Arrange
        var users = new List<User> { CreateUser(), CreateUser(), CreateUser() };
        var pagedResult = new PagedResult<User>
        {
            Items = users.ToArray(),
            TotalCount = 100, // In total DB contains 100
            Page = 2,
            PageSize = 3
        };

        UserRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<ACriteria<User>>(),
                It.IsAny<PaginationCriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        MapperMock
            .Setup(m => m.Map<UserListItemModel>(It.IsAny<User>()))
            .Returns(new UserListItemModel());

        // Act
        var result = await _handler.Handle(
            new GetUsersRequest { Page = 2, PageSize = 3 },
            CancellationToken.None);

        // Assert
        Assert.Equal(100, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(3, result.PageSize);
    }
}