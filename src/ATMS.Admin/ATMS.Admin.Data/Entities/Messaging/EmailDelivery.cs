using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Messaging;

public class EmailDelivery : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; }

    public int Type { get; set; }

    public string? TemporaryPassword { get; set; }

    public string? InviterName { get; set; }

    public string? ProjectTitle { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public int Status { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? LastError { get; set; }
}
