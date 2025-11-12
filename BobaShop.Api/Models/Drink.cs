// -----------------------------------------------------------------------------
// File: Models/Drink.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Represents a drink item in the BobaShop menu stored in MongoDB.
//   Includes base and size-based pricing, default sugar/ice options, 
//   availability flag, image metadata, and lifecycle tracking fields
//   following the “Magic Three Dates” pattern (CreatedUtc, UpdatedUtc, DeletedUtc).
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BobaShop.Api.Models
{
    // -------------------------------------------------------------------------
    // Model: Drink
    // Purpose:
    //   Defines a single drink entry available in the BobaShop menu.
    //   Used in MongoDB CRUD operations for menu management and orders.
    //   Supports “soft delete” logic via DeletedUtc field.
    // Mapping:
    //   ICTPRG554 PE1.1 / PE1.2 / ICTPRG556 PE2.1
    // -------------------------------------------------------------------------
    public class Drink
    {
        // ---------------------------------------------------------------------
        // Primary Key (MongoDB ObjectId)
        // ---------------------------------------------------------------------
        // - Automatically generated when the object is created.
        // - Stored as a string for JSON serialization and API response safety.
        // ---------------------------------------------------------------------
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        // ---------------------------------------------------------------------
        // Display fields
        // ---------------------------------------------------------------------

        // Name of the drink as it appears in the menu.
        // Example: "Brown Sugar Milk Tea"
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        // Short text description shown on product pages or menus.
        // Example: "Rich black tea with fresh milk and caramelized brown sugar syrup."
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        // ---------------------------------------------------------------------
        // Pricing fields
        // ---------------------------------------------------------------------

        // Base price for the smallest size.
        // - Mapped to MongoDB Decimal128 for currency precision.
        // - Used as the base for upcharge calculations.
        [BsonElement("basePrice")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal BasePrice { get; set; } = 6.00m;

        // Effective display price (often equal to BasePrice).
        // - Used when calculating totals for customer orders.
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; } = 6.00m;

        // Optional additional cost for small size variant.
        [BsonElement("smallUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal SmallUpcharge { get; set; } = 0.00m;

        // Additional cost for medium size variant.
        [BsonElement("mediumUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal MediumUpcharge { get; set; } = 0.50m;

        // Additional cost for large size variant.
        [BsonElement("largeUpcharge")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal LargeUpcharge { get; set; } = 1.00m;

        // ---------------------------------------------------------------------
        // Defaults (customization preferences)
        // ---------------------------------------------------------------------

        // Default sugar level (0–100%).
        // - Represents the preset level shown to customers when ordering.
        [BsonElement("defaultSugar")]
        public int DefaultSugar { get; set; } = 50;

        // Default ice level (0–100%).
        // - Represents the default ice preference for the drink.
        [BsonElement("defaultIce")]
        public int DefaultIce { get; set; } = 50;

        // Indicates whether the drink is currently available.
        // - Set to false for discontinued or seasonal items.
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // ---------------------------------------------------------------------
        // Image fields
        // ---------------------------------------------------------------------

        // Optional URL to the drink’s image file (e.g., stored under /images/drinks/).
        [BsonElement("imageUrl")]
        [BsonIgnoreIfNull]
        public string? ImageUrl { get; set; }

        // Alternative text for accessibility and SEO.
        // Example: "Cup of matcha milk tea with cream foam"
        [BsonElement("imageAlt")]
        [BsonIgnoreIfNull]
        public string? ImageAlt { get; set; }

        // ---------------------------------------------------------------------
        // Lifecycle fields (Magic Three Dates pattern)
        // ---------------------------------------------------------------------
        // Purpose:
        //   Provides consistent tracking for record creation, updates,
        //   and soft deletions. Used throughout the project for data integrity.
        // ---------------------------------------------------------------------

        // UTC timestamp when this record was first created.
        [BsonElement("createdUtc")]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // UTC timestamp when this record was last modified.
        [BsonElement("updatedUtc")]
        public DateTime? UpdatedUtc { get; set; }

        // UTC timestamp marking the record as “soft deleted”.
        // - Null means the record is still active.
        [BsonElement("deletedUtc")]
        public DateTime? DeletedUtc { get; set; }

        // ---------------------------------------------------------------------
        // Legacy compatibility properties
        // ---------------------------------------------------------------------
        // These aliases mirror older schema conventions (CreatedAt, etc.).
        // - Ignored during MongoDB serialization to avoid redundancy.
        // ---------------------------------------------------------------------
        [BsonIgnore] public DateTime CreatedAt { get => CreatedUtc; set => CreatedUtc = value; }
        [BsonIgnore] public DateTime? UpdatedAt { get => UpdatedUtc; set => UpdatedUtc = value; }
        [BsonIgnore] public DateTime? DeletedAt { get => DeletedUtc; set => DeletedUtc = value; }
    }
}
