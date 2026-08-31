using ATMS.Caching.Constants;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Requests.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTickets;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTickets;

public class GetWorkTicketHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenCacheMiss_LoadsMapsAndCachesWithEntityTtl()
    {
        var request = new GetWorkTicketRequest
        {
            ProjectId = Guid.NewGuid(),
            WorkTicketId = Guid.NewGuid()
        };
        var entity = new WorkTicket { Id = request.WorkTicketId };
        var expected = CreateModel(request.WorkTicketId, request.ProjectId);
        SetupCacheMiss<WorkTicketModel>();
        WorkTicketRepositoryMock
            .Setup(repository => repository.GetAsync(
                request.ProjectId,
                request.WorkTicketId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        MapperMock.Setup(mapper => mapper.Map<WorkTicketModel>(entity)).Returns(expected);
        var handler = new GetWorkTicketHandler(
            WorkTicketRepositoryMock.Object,
            CacheServiceMock.Object,
            MapperMock.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Same(expected, result);
        CacheServiceMock.Verify(cache => cache.GetOrSetAsync(
            CacheKeys.Project.TicketById(request.WorkTicketId),
            It.IsAny<Func<Task<WorkTicketModel>>>(),
            CacheTtl.Entity,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_DoesNotQueryRepository()
    {
        var projectId = Guid.NewGuid();
        var expected = CreateModel(Guid.NewGuid(), projectId);
        SetupCacheHit(expected);
        var handler = new GetWorkTicketHandler(
            WorkTicketRepositoryMock.Object,
            CacheServiceMock.Object,
            MapperMock.Object);

        var result = await handler.Handle(new GetWorkTicketRequest
        {
            ProjectId = projectId,
            WorkTicketId = expected.Id
        }, CancellationToken.None);

        Assert.Same(expected, result);
        WorkTicketRepositoryMock.Verify(
            repository => repository.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCachedTicketBelongsToAnotherProject_ThrowsNotFound()
    {
        var expected = CreateModel(Guid.NewGuid(), Guid.NewGuid());
        SetupCacheHit(expected);
        var handler = new GetWorkTicketHandler(
            WorkTicketRepositoryMock.Object,
            CacheServiceMock.Object,
            MapperMock.Object);

        await Assert.ThrowsAsync<ATMS.Application.Exceptions.Entity.EntityException>(() => handler.Handle(
            new GetWorkTicketRequest { ProjectId = Guid.NewGuid(), WorkTicketId = expected.Id },
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTicketDoesNotExist_ThrowsNotFound()
    {
        SetupCacheMiss<WorkTicketModel>();
        WorkTicketRepositoryMock
            .Setup(repository => repository.GetAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTicket?)null);
        var handler = new GetWorkTicketHandler(
            WorkTicketRepositoryMock.Object,
            CacheServiceMock.Object,
            MapperMock.Object);

        await Assert.ThrowsAsync<ATMS.Application.Exceptions.Entity.EntityException>(() => handler.Handle(
            new GetWorkTicketRequest { ProjectId = Guid.NewGuid(), WorkTicketId = Guid.NewGuid() },
            CancellationToken.None));
    }

    private static WorkTicketModel CreateModel(Guid id, Guid? projectId = null) => new()
    {
        Id = id,
        WorkProjectId = projectId ?? Guid.NewGuid(),
        Code = "T-1",
        Title = "Ticket",
        MilestoneTitle = "Milestone",
        GroupTitle = "Group",
        WorkTicketType = new(),
        WorkTicketStatus = new(),
        Priority = new()
    };
}
