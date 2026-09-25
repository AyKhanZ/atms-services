using ATMS.Application.Realtime;
using ATMS.Project.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ATMS.Project.API.Realtime;

public sealed class SignalRRealtimeEventPublisher(IHubContext<RealtimeHub> hubContext) : IRealtimeEventPublisher
{
    public Task PublishToUsersAsync<T>(
        IEnumerable<Guid> userIds,
        string eventName,
        T payload,
        CancellationToken cancellationToken) =>
        hubContext.Clients.Users(userIds.Select(id => id.ToString()))
            .SendAsync(eventName, payload, cancellationToken);

    public Task PublishToTaskAsync<T>(
        Guid taskId,
        bool teamOnly,
        string eventName,
        T payload,
        CancellationToken cancellationToken) =>
        hubContext.Clients.Group(teamOnly
                ? RealtimeConstants.Groups.TaskTeam(taskId)
                : RealtimeConstants.Groups.Task(taskId))
            .SendAsync(eventName, payload, cancellationToken);

    public Task PublishToProjectAsync<T>(
        Guid projectId,
        string eventName,
        T payload,
        CancellationToken cancellationToken) =>
        hubContext.Clients.Group(RealtimeConstants.Groups.Project(projectId))
            .SendAsync(eventName, payload, cancellationToken);
}
