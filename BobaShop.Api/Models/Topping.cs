using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BobaShop.Api.Models;

public class Topping
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("name")]
    public string Name { get; set; } = default!;

    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; } = 0.80m;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    // Canonical fields used for persistence
    [BsonElement("createdUtc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedUtc")]
    public DateTime? UpdatedUtc { get; set; }

    [BsonElement("deletedUtc")]
    public DateTime? DeletedUtc { get; set; }

    // ------ Backward-compatible aliases (compile-only, not stored in Mongo) ------
    [BsonIgnore] public DateTime CreatedAt { get => CreatedUtc; set => CreatedUtc = value; }
    [BsonIgnore] public DateTime? UpdatedAt { get => UpdatedUtc; set => UpdatedUtc = value; }
    [BsonIgnore] public DateTime? DeletedAt { get => DeletedUtc; set => DeletedUtc = value; }
}
