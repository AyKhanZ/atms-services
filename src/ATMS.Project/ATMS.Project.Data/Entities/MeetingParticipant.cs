using ATMS.Data;
using ATMS.Data.Enums;

namespace ATMS.Project.Data.Entities;

public class MeetingParticipant : BaseEntity
{
    public Guid MeetingId { get; set; }

    public Meeting Meeting { get; set; }

    public Guid ParticipantId { get; set; }

    public WorkProjectParticipant Participant { get; set; }

    public MeetingParticipantStatusEnum Status { get; set; }
}
