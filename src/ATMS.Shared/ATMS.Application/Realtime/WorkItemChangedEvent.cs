namespace ATMS.Application.Realtime;

public sealed record WorkItemChangedEvent(Guid ProjectId, string EntityType, Guid Id, string Action);
