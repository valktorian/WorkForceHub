using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EvolutionService.Query.Domain;

[BsonIgnoreExtraElements]
public class SalaryChangeReadModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("employeeId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EmployeeId { get; set; }

    [BsonElement("previousSalary")]
    public decimal PreviousSalary { get; set; }

    [BsonElement("newSalary")]
    public decimal NewSalary { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = string.Empty;

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
