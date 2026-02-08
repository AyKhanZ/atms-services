using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ATMS.Admin.Data.Entities;

public abstract class BaseDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
}