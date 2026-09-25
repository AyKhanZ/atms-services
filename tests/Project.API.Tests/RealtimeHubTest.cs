using ATMS.Application.Realtime;
using ATMS.Data.Enums;
using ATMS.Project.API.Hubs;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Security.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Project.API.Tests;

public sealed class RealtimeHubTest
{
    private readonly Mock<IProjectPermissionService> _permissions = new();
    private readonly Mock<IWorkTaskRepository> _tasks = new();
    private readonly Mock<IGroupManager> _groups = new();
    private readonly Mock<HubCallerContext> _context = new();

    public RealtimeHubTest()
    {
        _context.SetupGet(context => context.ConnectionId).Returns("connection-1");
        _context.SetupGet(context => context.Items).Returns(new Dictionary<object, object?>());
        _context.SetupGet(context => context.ConnectionAborted).Returns(CancellationToken.None);
        _groups.Setup(groups => groups.AddToGroupAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _groups.Setup(groups => groups.RemoveFromGroupAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task JoinProject_WithoutProjectView_DoesNotJoin()
    {
        var projectId = Guid.NewGuid();
        AllowProject(projectId, allowed: false);

        await CreateHub().JoinProject(projectId);

        _groups.Verify(groups => groups.AddToGroupAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WatchTask_ClientJoinsTaskButNeverTeam()
    {
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        AllowProject(projectId, allowed: true);
        _tasks.Setup(tasks => tasks.IsWorkTaskExistAsync(projectId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _permissions.Setup(service => service.IsClientAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateHub().WatchTask(projectId, taskId);

        _groups.Verify(groups => groups.AddToGroupAsync(
            "connection-1", RealtimeConstants.Groups.Task(taskId), It.IsAny<CancellationToken>()), Times.Once);
        _groups.Verify(groups => groups.AddToGroupAsync(
            "connection-1", RealtimeConstants.Groups.TaskTeam(taskId), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WatchTask_EmployeeJoinsTaskAndTeam()
    {
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        AllowProject(projectId, allowed: true);
        _tasks.Setup(tasks => tasks.IsWorkTaskExistAsync(projectId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _permissions.Setup(service => service.IsClientAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateHub().WatchTask(projectId, taskId);

        _groups.Verify(groups => groups.AddToGroupAsync(
            "connection-1", RealtimeConstants.Groups.TaskTeam(taskId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task JoinProject_AtGroupLimit_DoesNotJoinMoreGroups()
    {
        var hub = CreateHub();
        for (var i = 0; i < 30; i++)
        {
            var projectId = Guid.NewGuid();
            AllowProject(projectId, allowed: true);
            await hub.JoinProject(projectId);
        }

        var additionalProject = Guid.NewGuid();
        AllowProject(additionalProject, allowed: true);
        await hub.JoinProject(additionalProject);

        _groups.Verify(groups => groups.AddToGroupAsync(
            "connection-1", RealtimeConstants.Groups.Project(additionalProject), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void AllowProject(Guid projectId, bool allowed) =>
        _permissions.Setup(service => service.HasAnyPermissionAsync(
                projectId,
                It.Is<IReadOnlyCollection<ProjectPermissionEnum>>(values => values.Contains(ProjectPermissionEnum.ProjectView)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowed);

    private RealtimeHub CreateHub() => new(_permissions.Object, _tasks.Object)
    {
        Context = _context.Object,
        Groups = _groups.Object
    };
}
