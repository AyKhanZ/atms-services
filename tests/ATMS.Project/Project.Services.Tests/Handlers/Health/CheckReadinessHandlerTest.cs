using ATMS.Infrastructure.Files;
using ATMS.Project.Contracts.Requests.Health;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Health;
using Moq;

namespace Project.Services.Tests.Handlers.Health;

public class CheckReadinessHandlerTest
{
    private readonly Mock<IHealthRepository> _healthRepositoryMock = new();
    private readonly Mock<IFileStorage> _fileStorageMock = new();

    // An instance without the shared attachments volume would save files the others cannot read;
    // it must not be ready, whatever the database says.
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public async Task Handle_IsReadyOnlyWhenDatabaseAndFileStorageAre(bool database, bool storage, bool ready)
    {
        _healthRepositoryMock.Setup(repository => repository.IsReadyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(database);
        _fileStorageMock.Setup(fileStorage => fileStorage.IsWritableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(storage);

        var result = await new CheckReadinessHandler(_healthRepositoryMock.Object, _fileStorageMock.Object)
            .Handle(new CheckReadinessRequest(), CancellationToken.None);

        Assert.Equal(ready, result);
    }
}
