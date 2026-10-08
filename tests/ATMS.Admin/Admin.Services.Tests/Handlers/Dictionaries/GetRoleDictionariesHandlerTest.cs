using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Dictionaries;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using Moq;

namespace Admin.Services.Tests.Handlers.Dictionaries;

public class GetRoleDictionariesHandlerTest : BaseHandlerTest
{
    private readonly GetRoleDictionariesHandler _handler;

    public GetRoleDictionariesHandlerTest()
    {
        _handler = new GetRoleDictionariesHandler(RoleRepositoryMock.Object, CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheHasRoles_ReturnsCachedRoles()
    {
        var expected = new[]
        {
            new DictionaryModel<Guid> { Id = Guid.NewGuid(), Name = "Employee", Code = "Employee" }
        };

        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                CacheKeys.Admin.AllRoles,
                It.IsAny<Func<Task<DictionaryModel<Guid>[]>>>(),
                CacheTtl.Dictionary,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(new GetRoleDictionariesRequest(), CancellationToken.None);

        Assert.Equal(expected, result);
        RoleRepositoryMock.Verify(r => r.GetAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_MapsRolesFromRepository()
    {
        var employeeId = Guid.NewGuid();
        var clientManagerId = Guid.NewGuid();
        var roles = new List<Role>
        {
            new() { Id = clientManagerId, Name = "Client Manager" },
            new() { Id = employeeId, Name = "Employee" }
        };

        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                CacheKeys.Admin.AllRoles,
                It.IsAny<Func<Task<DictionaryModel<Guid>[]>>>(),
                CacheTtl.Dictionary,
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<DictionaryModel<Guid>[]>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory());

        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var result = await _handler.Handle(new GetRoleDictionariesRequest(), CancellationToken.None);

        Assert.Collection(result,
            role =>
            {
                Assert.Equal(clientManagerId, role.Id);
                Assert.Equal("Client Manager", role.Name);
                Assert.Equal("Client Manager", role.Code);
            },
            role =>
            {
                Assert.Equal(employeeId, role.Id);
                Assert.Equal("Employee", role.Name);
                Assert.Equal("Employee", role.Code);
            });
    }

    [Fact]
    public async Task Handle_UsesAllRolesCacheKey()
    {
        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<DictionaryModel<Guid>[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _handler.Handle(new GetRoleDictionariesRequest(), CancellationToken.None);

        CacheServiceMock.Verify(c => c.GetOrSetAsync(
            CacheKeys.Admin.AllRoles,
            It.IsAny<Func<Task<DictionaryModel<Guid>[]>>>(),
            CacheTtl.Dictionary,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}