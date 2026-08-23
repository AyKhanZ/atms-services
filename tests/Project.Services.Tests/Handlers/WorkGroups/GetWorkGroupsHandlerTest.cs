using System.Linq.Expressions;
using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Models.WorkGroups;
using ATMS.Project.Contracts.Requests.WorkGroups;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkGroups;
using ATMS.Project.Services.Handlers.WorkGroups;
using Moq;

namespace Project.Services.Tests.Handlers.WorkGroups;

public class GetWorkGroupsHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenProjectExists_ReturnsNestedGroupsWithTicketCounts()
    {
        var projectId = Guid.NewGuid();
        var milestone = new WorkGroup { Id = Guid.NewGuid() };
        var entities = new[]
        {
            new WorkGroup
            {
                Id = Guid.NewGuid(),
                Children = [milestone]
            }
        };
        WorkGroupModel[] expected =
        [
            new WorkGroupModel
            {
                Id = entities[0].Id,
                Title = "Group",
                Status = new(),
                Milestones =
                [
                    new WorkGroupModel
                    {
                        Id = milestone.Id,
                        Title = "Milestone",
                        ParentWorkGroupId = entities[0].Id,
                        Status = new(),
                        Milestones = [],
                        TicketCount = 0
                    }
                ],
                TicketCount = 0
            }
        ];
        var queryResult = new WorkGroupsQueryResult(
            entities,
            new Dictionary<Guid, int>
            {
                [entities[0].Id] = 2,
                [milestone.Id] = 3
            });
        WorkProjectRepositoryMock
            .Setup(x => x.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        WorkGroupRepositoryMock
            .Setup(x => x.GetGroupsAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);
        MapperMock.Setup(x => x.Map<WorkGroupModel[]>(entities)).Returns(expected);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new GetWorkGroupsRequest { ProjectId = projectId },
            CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal(2, result[0].TicketCount);
        Assert.Equal(3, result[0].Milestones[0].TicketCount);
    }

    [Fact]
    public async Task Handle_WhenProjectDoesNotExist_ThrowsNotFound()
    {
        WorkProjectRepositoryMock
            .Setup(x => x.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<EntityException>(() => handler.Handle(
            new GetWorkGroupsRequest { ProjectId = Guid.NewGuid() },
            CancellationToken.None));

        WorkGroupRepositoryMock.Verify(
            x => x.GetGroupsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GetWorkGroupsHandler CreateHandler()
    {
        return new GetWorkGroupsHandler(
            WorkProjectRepositoryMock.Object,
            WorkGroupRepositoryMock.Object,
            MapperMock.Object);
    }
}
