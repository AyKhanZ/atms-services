using ATMS.Data;
using ATMS.Data.Enums;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class Meeting : AuditableEntity, ISoftDeletable
{
    public string Title { get; set; }

    public string? Description { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public string? Location { get; set; }

    public string? MeetingUrl { get; set; }

    public MeetingStatusEnum Status { get; set; }

    public Guid WorkProjectId { get; set; }

    public WorkProject WorkProject { get; set; }

    public Guid? WorkTicketId { get; set; }

    public WorkTicket? WorkTicket { get; set; }

    public ICollection<MeetingParticipant> Participants { get; set; } = [];

    public ICollection<MeetingAgendaItem> AgendaItems { get; set; } = [];

    public ICollection<MeetingMinute> Minutes { get; set; } = [];

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedById { get; set; }
}
