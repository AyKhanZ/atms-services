using System.ComponentModel.DataAnnotations;

namespace ATMS.Infrastructure.Options;

public sealed class NotificationsOptions
{
    public TimeOnly DeadlineReminderTime { get; init; } = new(9, 0);

    public TimeOnly CleanupTime { get; init; } = new(3, 0);

    public int ReadRetentionDays { get; init; } = 30;

    public int RetentionDays { get; init; } = 90;

    // off while smtp is a test account with a small limit; off = no email rows, so no backlog later
    public bool SendEmails { get; init; }

    public int MaxEmailsPerUserPerDay { get; init; } = 50;

    public int MaxEmailsPerDay { get; init; } = 300;

    // frontend url for the links in emails
    [Required]
    public required string AppUrl { get; init; }
}
