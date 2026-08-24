using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class Meeting : SoftDeletableAuditableEntity<User>
{
    public string Title { get; set; }

    public string? Description { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public string? Location { get; set; }

    public string? MeetingUrl { get; set; }

    public int Status { get; set; }

    public Guid WorkProjectId { get; set; }

    public WorkProject WorkProject { get; set; }

    public Guid? WorkTicketId { get; set; }

    public WorkTicket? WorkTicket { get; set; }

    public ICollection<MeetingParticipant> Participants { get; set; } = [];

    public ICollection<MeetingAgendaItem> AgendaItems { get; set; } = [];

    public ICollection<MeetingMinute> Minutes { get; set; } = [];
}
