namespace ATMS.Infrastructure.Options;

public class NotificationsOptions
{
    public TimeOnly DeadlineReminderTime { get; init; } = new(9, 0);

    public TimeOnly CleanupTime { get; init; } = new(3, 0);

    public int ReadRetentionDays { get; init; } = 30;

    public int RetentionDays { get; init; } = 90;

    // Off while the SMTP account is a test one with a small monthly limit. Off means no email rows
    // are written and nothing is sent, so switching it on later does not send a backlog.
    public bool SendEmails { get; init; }

    // The interface the links in emails lead to.
    public required string AppUrl { get; init; }
}
