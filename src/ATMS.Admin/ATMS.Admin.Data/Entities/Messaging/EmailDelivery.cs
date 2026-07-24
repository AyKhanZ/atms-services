using ATMS.Data.Enums;

namespace ATMS.Admin.Data.Entities.Messaging;

public class EmailDelivery
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }

    public EmailDeliveryTypeEnum Type { get; set; }

    public string? TemporaryPassword { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public DeliveryStatusEnum Status { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? LastError { get; set; }
}
