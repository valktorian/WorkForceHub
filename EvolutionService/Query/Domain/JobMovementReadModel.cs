using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EvolutionService.Query.Domain;

[BsonIgnoreExtraElements]
public class JobMovementReadModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("employeeId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EmployeeId { get; set; }

    [BsonElement("previousJobTitle")]
    public string PreviousJobTitle { get; set; } = string.Empty;

    [BsonElement("newJobTitle")]
    public string NewJobTitle { get; set; } = string.Empty;

    [BsonElement("previousDepartment")]
    public string PreviousDepartment { get; set; } = string.Empty;

    [BsonElement("newDepartment")]
    public string NewDepartment { get; set; } = string.Empty;

    [BsonElement("effectiveDate")]
    public DateTime EffectiveDate { get; set; }

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty;

    [BsonElement("comment")]
    public string? Comment { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
