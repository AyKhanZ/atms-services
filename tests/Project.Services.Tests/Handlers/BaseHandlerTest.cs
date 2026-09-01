using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Infrastructure.Images;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Services.Interfaces;
using AutoMapper;
using Bogus;
using Moq;

namespace Project.Services.Tests.Handlers;

public abstract class BaseHandlerTest
{
    protected readonly Faker Faker = new();

    protected readonly Mock<IMapper> MapperMock = new();
    protected readonly Mock<ICurrentUser> CurrentUserMock = new();
    protected readonly Mock<ICacheService> CacheServiceMock = new();
    protected readonly Mock<IImageStorage> ImageStorageMock = new();

    protected readonly Mock<IDictionariesRepository> DictionariesRepositoryMock = new();
    protected readonly Mock<IOrganizationRepository> OrganizationRepositoryMock = new();
    protected readonly Mock<IWorkProjectRepository> WorkProjectRepositoryMock = new();
    protected readonly Mock<IWorkGroupRepository> WorkGroupRepositoryMock = new();
    protected readonly Mock<IWorkTicketRepository> WorkTicketRepositoryMock = new();
    protected readonly Mock<IEntityCodeGenerator> EntityCodeGeneratorMock = new();
    
    // Simulates cache miss — factory is called, repository will be hit
    protected void SetupCacheMiss<T>()
    {
        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<T>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<T>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory());
    }

    // Simulates cache hit — returns provided value, repository is never called
    protected void SetupCacheHit<T>(T value)
    {
        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<T>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(value);
    }
}
