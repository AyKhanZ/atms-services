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
        var model = CreateModel(request.Id);

        if (cacheHit)
        {
            SetupCacheHit(model);
        }
        else
        {
            SetupCacheMiss<WorkProjectModel>();
            SetupProject(request.Id, model, []);
        }

        var result = await CreateHandler().Handle(request, CancellationToken.None);

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

    [Fact]
    public async Task Handle_CacheMiss_ReturnsLivePendingInvitations()
    {
        var request = new GetWorkProjectRequest { Id = Guid.NewGuid() };
        var model = CreateModel(request.Id);
        WorkProjectInvitation[] invitations = [new() { Id = Guid.NewGuid(), WorkProjectId = request.Id }];
        WorkProjectInvitationModel[] invitationModels = [new() { Id = invitations[0].Id }];
        SetupCacheMiss<WorkProjectModel>();
        SetupProject(request.Id, model, invitations);
        MapperMock
            .Setup(mapper => mapper.Map<WorkProjectInvitationModel[]>(
                It.Is<List<WorkProjectInvitation>>(list => list.SequenceEqual(invitations))))
            .Returns(invitationModels);

        var result = await CreateHandler().Handle(request, CancellationToken.None);

        Assert.Same(invitationModels, result.Invitations);
        WorkProjectInvitationRepositoryMock.Verify(repository => repository.GetLivePendingAsync(
            request.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private GetWorkProjectHandler CreateHandler()
    {
        return new GetWorkProjectHandler(
            CurrentUserMock.Object,
            WorkProjectRepositoryMock.Object,
            WorkProjectInvitationRepositoryMock.Object,
            CacheServiceMock.Object,
            MapperMock.Object);
    }

    private void SetupProject(Guid id, WorkProjectModel model, WorkProjectInvitation[] invitations)
    {
        var entity = new WorkProject { Id = id };
        WorkProjectRepositoryMock
            .Setup(repository => repository.GetAsync(
                id,
                It.IsAny<AccessibleWorkProjectsCriteria>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        WorkProjectInvitationRepositoryMock
            .Setup(repository => repository.GetLivePendingAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitations.ToList());
        MapperMock.Setup(mapper => mapper.Map<WorkProjectModel>(entity)).Returns(model);
    }

    private static WorkProjectModel CreateModel(Guid id)
    {
        return new WorkProjectModel
        {
            Id = id,
            Code = "P-1",
            Title = "Project",
            ProjectType = new(),
            ProjectKind = new(),
            ProjectStatus = new()
        };
    }
}
