using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EvolutionService.Query.Domain;

[BsonIgnoreExtraElements]
public class ProfileLookupReadModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("accountId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid? AccountId { get; set; }
}
