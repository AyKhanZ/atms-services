using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class MeetingMinute : AuditableEntity
{
    public Guid MeetingId { get; set; }

    public Meeting Meeting { get; set; }

    public string Text { get; set; }

    public uint Order { get; set; }
}
