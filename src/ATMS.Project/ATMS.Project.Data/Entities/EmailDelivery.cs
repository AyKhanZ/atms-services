using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class EmailDelivery : BaseEntity
{
    public Guid NotificationId { get; set; }

    public Notification Notification { get; set; }

    public int Status { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? LastError { get; set; }
}
