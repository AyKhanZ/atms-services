using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class HistoryEntry : BaseEntity
{
    public Guid WorkProjectId { get; set; }

    public int EntityType { get; set; }

    public Guid EntityId { get; set; }

    public int Action { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedById { get; set; }


    public ICollection<HistoryChange> Changes { get; set; } = [];
}
