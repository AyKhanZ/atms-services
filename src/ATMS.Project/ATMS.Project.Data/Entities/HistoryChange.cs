using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class HistoryChange : BaseEntity
{
    public Guid HistoryEntryId { get; set; }

    public HistoryEntry HistoryEntry { get; set; }


    public int Field { get; set; }

    public Guid? SubjectId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}
