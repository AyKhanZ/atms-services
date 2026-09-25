using ATMS.Application.Realtime;
using ATMS.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ATMS.Project.API.Hubs;

[Authorize]
public sealed class RealtimeHub(
    IProjectPermissionService permissions,
    IWorkTaskRepository tasks) : Hub
{
    private const int MaxGroups = 30;
    private const string GroupsKey = "realtime.groups";

    /// <summary>Subscribes this connection to changes in a project the user can view.</summary>
    /// <param name="projectId">The project being viewed. Requests without access are ignored.</param>
    public async Task JoinProject(Guid projectId)
    {
        if (projectId == Guid.Empty || !await CanViewProjectAsync(projectId))
        {
            return;
        }

        await JoinAsync(RealtimeConstants.Groups.Project(projectId));
    }

    /// <summary>Stops receiving changes for the specified project on this connection.</summary>
    /// <param name="projectId">The project to leave.</param>
    public Task LeaveProject(Guid projectId) => LeaveAsync(RealtimeConstants.Groups.Project(projectId));

    /// <summary>Subscribes to task comment changes when the task belongs to a viewable project.</summary>
    /// <param name="projectId">The project containing the task.</param>
    /// <param name="taskId">The task being viewed. Requests without access are ignored.</param>
    public async Task WatchTask(Guid projectId, Guid taskId)
    {
        if (projectId == Guid.Empty || taskId == Guid.Empty || !await CanViewProjectAsync(projectId) ||
            !await tasks.IsWorkTaskExistAsync(projectId, taskId, Context.ConnectionAborted))
        {
            return;
        }

        var taskGroup = RealtimeConstants.Groups.Task(taskId);
        var teamGroup = RealtimeConstants.Groups.TaskTeam(taskId);
        var isClient = await permissions.IsClientAsync(projectId, Context.ConnectionAborted);
        var groups = ConnectionGroups;
        var needed = (groups.Contains(taskGroup) ? 0 : 1) +
                     (isClient || groups.Contains(teamGroup) ? 0 : 1);

        if (groups.Count + needed > MaxGroups)
        {
            return;
        }

        await JoinAsync(taskGroup);
        if (!isClient)
        {
            await JoinAsync(teamGroup);
        }
    }

    /// <summary>Stops receiving comment changes for the specified task on this connection.</summary>
    /// <param name="taskId">The task to stop watching.</param>
    public async Task UnwatchTask(Guid taskId)
    {
        await LeaveAsync(RealtimeConstants.Groups.Task(taskId));
        await LeaveAsync(RealtimeConstants.Groups.TaskTeam(taskId));
    }

    private Task<bool> CanViewProjectAsync(Guid projectId) =>
        permissions.HasAnyPermissionAsync(
            projectId,
            [ProjectPermissionEnum.ProjectView],
            Context.ConnectionAborted);

    private HashSet<string> ConnectionGroups
    {
        get
        {
            if (Context.Items.TryGetValue(GroupsKey, out var value) && value is HashSet<string> groups)
            {
                return groups;
            }

            groups = new HashSet<string>(StringComparer.Ordinal);
            Context.Items[GroupsKey] = groups;
            return groups;
        }
    }

    private async Task JoinAsync(string group)
    {
        var groups = ConnectionGroups;
        if (groups.Contains(group) || groups.Count >= MaxGroups)
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
        groups.Add(group);
    }

    private async Task LeaveAsync(string group)
    {
        var groups = ConnectionGroups;
        if (!groups.Contains(group))
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
        groups.Remove(group);
    }
}
