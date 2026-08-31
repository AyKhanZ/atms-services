using System.Linq.Expressions;
using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Requests.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Data.Criteria;
using ATMS.Project.Data.Criteria.WorkTickets;
using ATMS.Project.Services.Handlers.WorkTickets;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTickets;

public class GetWorkTicketsHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenProjectExists_ReturnsMappedTickets()
    {
        var request = new GetWorkTicketsRequest { ProjectId = Guid.NewGuid() };
        WorkProjectRepositoryMock
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var entity = new WorkTicket { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        var expected = new WorkTicketModel { Id = entity.Id };
        var entities = new KeysetPagedResult<WorkTicket>
        {
            Items = [entity],
            HasMore = true,
            NextCursor = "next",
            PageSize = 1
        };
        WorkTicketRepositoryMock
            .Setup(repository => repository.GetManyAsync(
                It.IsAny<WorkTicketsByProjectCriteria>(),
                It.IsAny<KeysetPaginationCriteria<WorkTicket>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
        MapperMock.Setup(mapper => mapper.Map<WorkTicketModel>(entity)).Returns(expected);
        var handler = new GetWorkTicketsHandler(
            WorkProjectRepositoryMock.Object,
            WorkTicketRepositoryMock.Object,
            MapperMock.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Same(expected, Assert.Single(result.Items));
        Assert.True(result.HasMore);
        Assert.Equal("next", result.NextCursor);
    }

    [Fact]
    public async Task Handle_WhenProjectDoesNotExist_ThrowsNotFound()
    {
        var request = new GetWorkTicketsRequest { ProjectId = Guid.NewGuid() };
        WorkProjectRepositoryMock
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new GetWorkTicketsHandler(
            WorkProjectRepositoryMock.Object,
            WorkTicketRepositoryMock.Object,
            MapperMock.Object);

        await Assert.ThrowsAsync<EntityException>(() =>
            handler.Handle(request, CancellationToken.None));

        WorkTicketRepositoryMock.Verify(repository => repository.GetManyAsync(
            It.IsAny<WorkTicketsByProjectCriteria>(),
            It.IsAny<KeysetPaginationCriteria<WorkTicket>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
