// -----------------------------------------------------------------------------
// File: Drink.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Represents a drink product available in the Boba Shop menu.
//   Includes price details, size upcharges, default customisation options,
//   and the “Magic Three Dates” fields to track record lifecycle (create,
//   update, delete). 
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BobaShop.Api.Models
{
    // -------------------------------------------------------------------------
    // Model: Drink
    // Purpose:
    //   Represents an individual drink item (e.g., Classic Milk Tea, Taro Latte).
    //   Stored in the MongoDB Drinks collection and linked to Orders by ID.
    //   Demonstrates schema-free data persistence and embedded audit tracking.
    // Mapping: ICTPRG554 PE1.1 / PE1.2 / ICTPRG556 PE2.1
    // -------------------------------------------------------------------------
    public class Drink
    {
        // Unique MongoDB identifier (ObjectId)
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        // -------------------------------------------------------------
        // Display and descriptive details
        // -------------------------------------------------------------

        // Display name of the drink shown in the menu 
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        // Short description used on the web UI or menu card
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        // -------------------------------------------------------------
        // Pricing details
        // -------------------------------------------------------------

        // Base price for the smallest/default size
        [BsonElement("basePrice")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal BasePrice { get; set; } = 6.00m;

        // Effective price (used in OrdersController for total calculation)
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; } = 6.00m;

        // Size-based upcharges (used when customer selects Medium/Large)
        [BsonElement("smallUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal SmallUpcharge { get; set; } = 0.00m;

        [BsonElement("mediumUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal MediumUpcharge { get; set; } = 0.50m;

        [BsonElement("largeUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal LargeUpcharge { get; set; } = 1.00m;

        // -------------------------------------------------------------
        // Default customisation settings
        // -------------------------------------------------------------

        // Default sugar level (percentage 0–100)
        [BsonElement("defaultSugar")]
        public int DefaultSugar { get; set; } = 50;

        // Default ice level (percentage 0–100)
        [BsonElement("defaultIce")]
        public int DefaultIce { get; set; } = 50;

        // Indicates whether this drink is visible and available for ordering
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // -------------------------------------------------------------
        // MAGIC THREE DATES (Record Lifecycle Tracking)
        // -------------------------------------------------------------

        // CreatedUtc: Date/time record was created (stored in UTC)
        [BsonElement("createdUtc")]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // UpdatedUtc: Date/time record was last modified
        [BsonElement("updatedUtc")]
        public DateTime? UpdatedUtc { get; set; }

        // DeletedUtc: Marks soft-deleted records (null means active)
        [BsonElement("deletedUtc")]
        public DateTime? DeletedUtc { get; set; }

        // -------------------------------------------------------------
        // Backward compatibility properties 
        // Purpose:
        //   Provide legacy alias properties for earlier schema versions.
        //   Useful when migrating existing data sets.
        // -------------------------------------------------------------
        [BsonIgnore] public DateTime CreatedAt { get => CreatedUtc; set => CreatedUtc = value; }
        [BsonIgnore] public DateTime? UpdatedAt { get => UpdatedUtc; set => UpdatedUtc = value; }
        [BsonIgnore] public DateTime? DeletedAt { get => DeletedUtc; set => DeletedUtc = value; }
    }
}
