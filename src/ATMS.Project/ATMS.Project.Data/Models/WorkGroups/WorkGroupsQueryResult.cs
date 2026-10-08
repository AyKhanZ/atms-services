using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Models.WorkGroups;

public sealed record WorkGroupsQueryResult(WorkGroup[] Groups, IReadOnlyDictionary<Guid, int> TicketCounts);
