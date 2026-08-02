using System.Text.Json;

namespace ATMS.Messaging.Models;

public sealed record MessageEnvelope(
    Guid MessageId,
    DateTime CreatedAt,
    string MessageType,
    JsonElement Payload);
