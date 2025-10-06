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
using System;

namespace BobaShop.Api.Models
{
    /// <summary>
    /// A drink available for purchase (e.g., Milk Tea, Taro, Matcha).
    /// </summary>
    public class Drink
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>Display name (e.g., "Classic Milk Tea").</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Short description shown in the UI.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Base price for the default size.</summary>
        public decimal BasePrice { get; set; } = 6.00m;

        // Size upcharges (0 means included in base).
        public decimal SmallUpcharge { get; set; } = 0.00m;
        public decimal MediumUpcharge { get; set; } = 0.50m;
        public decimal LargeUpcharge { get; set; } = 1.00m;

        /// <summary>Default sugar level (0–100%).</summary>
        public int DefaultSugar { get; set; } = 50;

        /// <summary>Default ice level (0–100%).</summary>
        public int DefaultIce { get; set; } = 50;

        /// <summary>Whether this drink is visible/available for ordering.</summary>
        public bool IsActive { get; set; } = true;

        // -------------------- Magic Three Dates --------------------
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }
        public DateTime? DeletedUtc { get; set; }
    }
}
