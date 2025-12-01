using Core.Abstractions.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Persistence.Mongo.Repositories;

public abstract class BaseDocument : IHasTimestamps
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    protected BaseDocument()
    {
        Id = ObjectId.GenerateNewId().ToString();
        CreatedDate = DateTime.UtcNow;
    }
}
