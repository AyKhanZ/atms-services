namespace ATMS.Application.Realtime;

public interface IRealtimeEventPublisher
{
    Task PublishToAdminsAsync(RealtimeEvent realtimeEvent, CancellationToken cancellationToken);

    Task PublishToOrganizationAsync(Guid organizationId, RealtimeEvent realtimeEvent, CancellationToken cancellationToken);

    Task PublishToProjectAsync(Guid projectId, RealtimeEvent realtimeEvent, CancellationToken cancellationToken);

    Task PublishToUsersAsync<T>(IEnumerable<Guid> userIds, string eventName, T payload, CancellationToken cancellationToken);
}
