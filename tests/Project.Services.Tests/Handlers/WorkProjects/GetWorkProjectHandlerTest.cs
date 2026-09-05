using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Contracts.Requests.WorkProjects;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkProjects;
using Moq;

namespace Project.Services.Tests.Handlers.WorkProjects;

public class GetWorkProjectHandlerTest : BaseHandlerTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_UsesLanguageSpecificEntityCache(bool cacheHit)
    {
        var request = new GetWorkProjectRequest { Id = Guid.NewGuid() };
        var model = new WorkProjectModel
        {
            Id = request.Id,
            Code = "P-1",
            Title = "Project",
            ProjectType = new(),
            ProjectKind = new(),
            ProjectStatus = new()
        };

        if (cacheHit)
        {
            SetupCacheHit(model);
        }
        else
        {
            SetupCacheMiss<WorkProjectModel>();
            var entity = new WorkProject { Id = request.Id };
            WorkProjectRepositoryMock
                .Setup(repository => repository.GetAsync(
                    request.Id,
                    It.IsAny<AccessibleWorkProjectsCriteria>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);
            MapperMock.Setup(mapper => mapper.Map<WorkProjectModel>(entity)).Returns(model);
        }

        var handler = new GetWorkProjectHandler(
            CurrentUserMock.Object,
            WorkProjectRepositoryMock.Object,
            CacheServiceMock.Object,
            MapperMock.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Same(model, result);
        CacheServiceMock.Verify(cache => cache.GetOrSetAsync(
            CacheKeys.Project.ProjectById(request.Id, CultureHelper.CurrentLanguage),
            It.IsAny<Func<Task<WorkProjectModel>>>(),
            CacheTtl.Entity,
            It.IsAny<CancellationToken>()), Times.Once);
        WorkProjectRepositoryMock.Verify(repository => repository.GetAsync(
            request.Id,
            It.IsAny<AccessibleWorkProjectsCriteria>(),
            It.IsAny<CancellationToken>()), cacheHit ? Times.Never : Times.Once);
    }
}
