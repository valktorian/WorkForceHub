using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EvolutionService.Query.Domain;

[BsonIgnoreExtraElements]
public class RewardReadModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("employeeId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EmployeeId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("value")]
    public decimal Value { get; set; }

    [BsonElement("grantedAt")]
    public DateTime GrantedAt { get; set; }

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty;

    [BsonElement("comment")]
    public string? Comment { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
