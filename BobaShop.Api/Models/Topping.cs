// -----------------------------------------------------------------------------
// File: Topping.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Represents an optional topping item that can be added to a drink order.
//   Includes name, price, active status, and “Magic Three Dates” fields to
//   record lifecycle events (created, updated, deleted).
//   Demonstrates document modelling and data persistence in MongoDB.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BobaShop.Api.Models
{
    // -------------------------------------------------------------------------
    // Model: Topping
    // Purpose:
    //   Defines a single topping entity used in the BobaShop API.
    //   Toppings are stored in the MongoDB collection and referenced in Orders.
    // Mapping: ICTPRG554 PE1.1 / PE1.2 / ICTPRG556 PE2.1
    // -------------------------------------------------------------------------
    public class Topping
    {
        // -------------------------------------------------------------
        // Primary Key
        // -------------------------------------------------------------
        // Unique MongoDB identifier (ObjectId)
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        // -------------------------------------------------------------
        // Core Data Fields
        // -------------------------------------------------------------
        // Display name of the topping 
        [BsonElement("name")]
        public string Name { get; set; } = default!;

        // Price in AUD (Decimal128 ensures precision in MongoDB)
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; } = 0.80m;

        // Indicates whether the topping is currently active and available
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // -------------------------------------------------------------
        // MAGIC THREE DATES (Record Lifecycle Tracking)
        // -------------------------------------------------------------
        // CreatedUtc: Date/time record was created (UTC)
        [BsonElement("createdUtc")]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // UpdatedUtc: Date/time record was last updated (UTC)
        [BsonElement("updatedUtc")]
        public DateTime? UpdatedUtc { get; set; }

        // DeletedUtc: Marks record as soft-deleted (null means active)
        [BsonElement("deletedUtc")]
        public DateTime? DeletedUtc { get; set; }

        // -------------------------------------------------------------
        // Backward Compatibility Properties 
        // Purpose:
        //   Provides alias property names for older schema references.
        //   Not stored in MongoDB; ignored during serialization.
        // -------------------------------------------------------------
        [BsonIgnore] public DateTime CreatedAt { get => CreatedUtc; set => CreatedUtc = value; }
        [BsonIgnore] public DateTime? UpdatedAt { get => UpdatedUtc; set => UpdatedUtc = value; }
        [BsonIgnore] public DateTime? DeletedAt { get => DeletedUtc; set => DeletedUtc = value; }
    }
}
