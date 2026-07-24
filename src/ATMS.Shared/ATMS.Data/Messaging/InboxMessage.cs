namespace ATMS.Data.Messaging;

public class InboxMessage
{
    public Guid MessageId { get; set; }

    public string ConsumerName { get; set; }

    public DateTime ProcessedAt { get; set; }
}
