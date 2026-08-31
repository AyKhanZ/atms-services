using System.Linq.Expressions;
using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkGroups;
using ATMS.Project.Contracts.Requests.WorkGroups;
using ATMS.Project.Data.Criteria.WorkGroups;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkGroups;
using Moq;

namespace Project.Services.Tests.Handlers.WorkGroups;

public class GetMilestonesHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenProjectExists_ReturnsMappedCursorPage()
    {
        var request = new GetMilestonesRequest
        {
            ProjectId = Guid.NewGuid(),
            Search = "release",
            PageSize = 10
        };
        var entity = new WorkGroup { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        var expected = new MilestoneOptionModel { Id = entity.Id };
        WorkProjectRepositoryMock
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        WorkGroupRepositoryMock
            .Setup(repository => repository.GetMilestonesAsync(
                It.IsAny<MilestonesByProjectCriteria>(),
                It.IsAny<KeysetPaginationCriteria<WorkGroup>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeysetPagedResult<WorkGroup>
            {
                Items = [entity],
                HasMore = true,
                NextCursor = "next",
                PageSize = 10
            });
        MapperMock.Setup(mapper => mapper.Map<MilestoneOptionModel>(entity)).Returns(expected);
        var handler = new GetMilestonesHandler(
            WorkProjectRepositoryMock.Object,
            WorkGroupRepositoryMock.Object,
            MapperMock.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Same(expected, Assert.Single(result.Items));
        Assert.True(result.HasMore);
        Assert.Equal("next", result.NextCursor);
    }

    [Fact]
    public async Task Handle_WhenProjectDoesNotExist_ThrowsAndDoesNotQueryMilestones()
    {
        WorkProjectRepositoryMock
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new GetMilestonesHandler(
            WorkProjectRepositoryMock.Object,
            WorkGroupRepositoryMock.Object,
            MapperMock.Object);

        await Assert.ThrowsAsync<EntityException>(() => handler.Handle(
            new GetMilestonesRequest { ProjectId = Guid.NewGuid() },
            CancellationToken.None));

        WorkGroupRepositoryMock.Verify(repository => repository.GetMilestonesAsync(
            It.IsAny<MilestonesByProjectCriteria>(),
            It.IsAny<KeysetPaginationCriteria<WorkGroup>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
