using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
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
    protected readonly Mock<IWorkTaskRepository> WorkTaskRepositoryMock = new();
    protected readonly Mock<IEntityCodeGenerator> EntityCodeGeneratorMock = new();

    protected BaseHandlerTest()
    {
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetProgressByTicketAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ATMS.Project.Data.Models.WorkTasks.WorkTaskProgress>());
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetIdsByTicketsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetChildIdsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        WorkTicketRepositoryMock
            .Setup(repository => repository.GetIdsByWorkGroupAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }
    
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

    protected void VerifyAllLocalizedCacheEntriesRemoved(Func<string, string> keyFactory)
    {
        foreach (var language in SupportedLanguages.All)
        {
            CacheServiceMock.Verify(
                cache => cache.RemoveAsync(keyFactory(language), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
