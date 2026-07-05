using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class MeetingAgendaItem : BaseEntity
{
    public Guid MeetingId { get; set; }

    public Meeting Meeting { get; set; }

    public string Title { get; set; }

    public uint Order { get; set; }
}
