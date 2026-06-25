using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EvolutionService.Query.Domain;

[BsonIgnoreExtraElements]
public class TrainingReadModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("employeeId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EmployeeId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("provider")]
    public string Provider { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    [BsonElement("endDate")]
    public DateTime? EndDate { get; set; }

    [BsonElement("completionDate")]
    public DateTime? CompletionDate { get; set; }

    [BsonElement("certificateUrl")]
    public string? CertificateUrl { get; set; }

    [BsonElement("comment")]
    public string? Comment { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
