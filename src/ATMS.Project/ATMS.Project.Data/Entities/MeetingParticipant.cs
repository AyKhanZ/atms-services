using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class MeetingParticipant : BaseEntity
{
    public Guid MeetingId { get; set; }

    public Meeting Meeting { get; set; }

    public Guid ParticipantId { get; set; }

    public WorkProjectParticipant Participant { get; set; }

    public int Status { get; set; }
}
