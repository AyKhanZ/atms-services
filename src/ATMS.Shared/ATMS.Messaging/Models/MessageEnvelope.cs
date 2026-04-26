namespace ATMS.Messaging.Models;

/*
 In practice: when you view a message in the RabbitMQ Management UI DLX,
 you should immediately see when it was created, its type, and its unique ID.
 Without this, you only see the payload and can’t tell when it happened.

 Without:
 { "Id": "abc", "Email": "test@test.com" }

 With:
 {
   "MessageId": "uuid",
   "CreatedAt": "2025-01-01T10:00:00",
   "MessageType": "UserCreatedEvent",
   "Payload": { "Id": "abc", "Email": "test@test.com" }
 }
*/

public sealed class MessageEnvelope<T>
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string MessageType { get; init; } = typeof(T).Name;
    public required T Payload { get; init; }
}