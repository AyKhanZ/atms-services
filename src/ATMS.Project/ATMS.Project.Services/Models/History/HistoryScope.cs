using ATMS.Data.Enums;

namespace ATMS.Project.Services.Models.History;

public sealed record HistoryScope(HistoryEntityTypeEnum EntityType, Guid EntityId);
