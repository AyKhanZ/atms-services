namespace ATMS.Application.Realtime;

public interface IRealtimeEventPublisher
{
    Task PublishToUsersAsync<T>(IEnumerable<Guid> userIds, string eventName, T payload, CancellationToken cancellationToken);

    Task PublishToTaskAsync<T>(Guid taskId, bool teamOnly, string eventName, T payload, CancellationToken cancellationToken);

    Task PublishToProjectAsync<T>(Guid projectId, string eventName, T payload, CancellationToken cancellationToken);
}
