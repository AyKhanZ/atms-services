namespace ATMS.Project.Contracts.Models.Notifications;

public class NotificationParametersModel
{
    public string? ProjectTitle { get; set; }

    public string? TaskCode { get; set; }

    public string? TaskTitle { get; set; }

    public int? TaskKind { get; set; }

    public int? FromStatusId { get; set; }

    public int? ToStatusId { get; set; }

    public DateOnly? Deadline { get; set; }
}
