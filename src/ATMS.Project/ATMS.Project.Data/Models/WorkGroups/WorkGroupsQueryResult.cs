using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Models.WorkGroups;

public record WorkGroupsQueryResult(WorkGroup[] Groups, IReadOnlyDictionary<Guid, int> TicketCounts);
