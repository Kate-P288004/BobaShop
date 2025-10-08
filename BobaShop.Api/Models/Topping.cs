using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BobaShop.Api.Models;

/// <summary>
/// Represents a drink topping (e.g., pearls, jelly, pudding)
/// Includes surcharge, status, and audit timestamps.
/// </summary>
public class Topping
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("name")]
    public string Name { get; set; } = default!;

    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; } = 0.80m;   // default surcharge

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    // Magic Three Dates
    [BsonElement("createdUtc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedUtc")]
    public DateTime? UpdatedUtc { get; set; }

    [BsonElement("deletedUtc")]
    public DateTime? DeletedUtc { get; set; }
}
