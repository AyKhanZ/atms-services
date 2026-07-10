namespace ATMS.Application.Realtime;

public sealed record RealtimeEvent(
    string Name,
    Guid EntityId,
    DateTime OccurredAt);
