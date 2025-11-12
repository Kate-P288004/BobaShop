using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BobaShop.Api.Models
{
    public class Drink
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        // Display
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        // Pricing
        [BsonElement("basePrice")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal BasePrice { get; set; } = 6.00m;

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; } = 6.00m;

        [BsonElement("smallUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal SmallUpcharge { get; set; } = 0.00m;

        [BsonElement("mediumUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal MediumUpcharge { get; set; } = 0.50m;

        [BsonElement("largeUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal LargeUpcharge { get; set; } = 1.00m;

        // Defaults
        [BsonElement("defaultSugar")]
        public int DefaultSugar { get; set; } = 50;

        [BsonElement("defaultIce")]
        public int DefaultIce { get; set; } = 50;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // Images (single definition)
        [BsonElement("imageUrl")]
        [BsonIgnoreIfNull]
        public string? ImageUrl { get; set; }

        [BsonElement("imageAlt")]
        [BsonIgnoreIfNull]
        public string? ImageAlt { get; set; }

        // Magic three dates
        [BsonElement("createdUtc")]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedUtc")]
        public DateTime? UpdatedUtc { get; set; }

        [BsonElement("deletedUtc")]
        public DateTime? DeletedUtc { get; set; }

        // Legacy aliases (optional)
        [BsonIgnore] public DateTime CreatedAt { get => CreatedUtc; set => CreatedUtc = value; }
        [BsonIgnore] public DateTime? UpdatedAt { get => UpdatedUtc; set => UpdatedUtc = value; }
        [BsonIgnore] public DateTime? DeletedAt { get => DeletedUtc; set => DeletedUtc = value; }
    }
}
