// -----------------------------------------------------------------------------
// File: Drink.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: October 2025
// Assessment: Diploma of IT – Application Development Project
// Description:
// Domain model representing a store drink with pricing and default
// customisation options. Includes the "Magic Three Dates" audit fields.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BobaShop.Api.Models
{
    /// <summary>
    /// A drink available for purchase (e.g., Milk Tea, Taro, Matcha).
    /// </summary>
    public class Drink
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>Display name (e.g., "Classic Milk Tea").</summary>
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Short description shown in the UI.</summary>
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Base price for the default size.</summary>
        [BsonElement("basePrice")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal BasePrice { get; set; } = 6.00m;

        /// <summary>Effective price used when ordering (what OrdersController sums).</summary>
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; } = 6.00m;

        // Size upcharges (0 means included in base).
        [BsonElement("smallUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal SmallUpcharge { get; set; } = 0.00m;

        [BsonElement("mediumUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal MediumUpcharge { get; set; } = 0.50m;

        [BsonElement("largeUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal LargeUpcharge { get; set; } = 1.00m;

        /// <summary>Default sugar level (0–100%).</summary>
        [BsonElement("defaultSugar")]
        public int DefaultSugar { get; set; } = 50;

        /// <summary>Default ice level (0–100%).</summary>
        [BsonElement("defaultIce")]
        public int DefaultIce { get; set; } = 50;

        /// <summary>Whether this drink is visible/available for ordering.</summary>
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // ---------------------------------------------------------------------
        // MAGIC THREE DATES (UTC)
        // ---------------------------------------------------------------------
        [BsonElement("createdUtc")]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedUtc")]
        public DateTime? UpdatedUtc { get; set; }

        [BsonElement("deletedUtc")]
        public DateTime? DeletedUtc { get; set; }

        // Back-compat aliases (can delete later if not needed anywhere else)
        [BsonIgnore] public DateTime CreatedAt { get => CreatedUtc; set => CreatedUtc = value; }
        [BsonIgnore] public DateTime? UpdatedAt { get => UpdatedUtc; set => UpdatedUtc = value; }
        [BsonIgnore] public DateTime? DeletedAt { get => DeletedUtc; set => DeletedUtc = value; }
    }
}
