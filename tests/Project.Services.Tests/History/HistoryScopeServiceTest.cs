using System.Linq.Expressions;
using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.History;
using FluentValidation;
using Moq;

namespace Project.Services.Tests.History;

public sealed class HistoryScopeServiceTest
{
    private readonly Mock<IWorkProjectRepository> _workProjectRepository = new();
    private readonly Mock<IWorkTaskRepository> _workTaskRepository = new();
    private readonly Guid _projectId = Guid.NewGuid();

    private HistoryScopeService Service() => new(_workProjectRepository.Object, _workTaskRepository.Object);

    [Fact]
    public async Task ResolveAsync_TicketAndTaskTogether_IsRefused()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Service().ResolveAsync(_projectId, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task ResolveAsync_Task_ReturnsTheTaskWhenItIsInTheProject()
    {
        var taskId = Guid.NewGuid();
        _workTaskRepository
            .Setup(repository => repository.IsWorkTaskExistAsync(_projectId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var scope = await Service().ResolveAsync(_projectId, null, taskId, CancellationToken.None);

        Assert.Equal(HistoryEntityTypeEnum.WorkTask, scope.EntityType);
        Assert.Equal(taskId, scope.EntityId);
    }

    [Fact]
    public async Task ResolveAsync_TaskOfAnotherProjectOrDeleted_IsNotFound()
    {
        _workTaskRepository
            .Setup(repository => repository.IsWorkTaskExistAsync(_projectId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            Service().ResolveAsync(_projectId, null, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }

    [Fact]
    public async Task ResolveAsync_Ticket_ReturnsTheTicket()
    {
        var ticketId = Guid.NewGuid();
        _workTaskRepository
            .Setup(repository => repository.IsWorkTicketExistAsync(_projectId, ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var scope = await Service().ResolveAsync(_projectId, ticketId, null, CancellationToken.None);

        Assert.Equal(new(HistoryEntityTypeEnum.WorkTicket, ticketId), scope);
    }

    [Fact]
    public async Task ResolveAsync_NeitherTicketNorTask_IsTheProject()
    {
        _workProjectRepository
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var scope = await Service().ResolveAsync(_projectId, null, null, CancellationToken.None);

        Assert.Equal(new(HistoryEntityTypeEnum.Project, _projectId), scope);
    }

    [Fact]
    public async Task ResolveAsync_MissingProject_IsNotFound()
    {
        _workProjectRepository
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<EntityException>(() =>
            Service().ResolveAsync(_projectId, null, null, CancellationToken.None));
    }
}
