using ATMS.Data.Enums;

namespace ATMS.Data.Messaging;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public string Exchange { get; set; }

    public string RoutingKey { get; set; }

    public string MessageType { get; set; }

    public string Payload { get; set; }

    public DeliveryStatusEnum Status { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? LastError { get; set; }
}
