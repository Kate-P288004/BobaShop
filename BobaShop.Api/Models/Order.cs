// -----------------------------------------------------------------------------
// File: Order.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Represents a customer order document stored in the MongoDB Orders collection.
//   Contains references to Drinks and Toppings, total price calculation,
//   customer email, order status, and Magic Three Dates for lifecycle tracking.
//   Demonstrates one-to-many relationships between collections in MongoDB.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace BobaShop.Api.Models
{
    // -------------------------------------------------------------------------
    // Model: Order
    // Purpose:
    //   Represents a customer order within the BobaShop API.
    //   Each order links to drink and topping IDs, and records total price,
    //   status, and lifecycle timestamps.
    // Mapping: ICTPRG554 PE1.1 / PE1.2 / ICTPRG556 PE2.1
    // -------------------------------------------------------------------------
    public class Order
    {
        // -------------------------------------------------------------
        // Primary Key
        // -------------------------------------------------------------
        // Unique MongoDB identifier (ObjectId)
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        // -------------------------------------------------------------
        // Customer Information
        // -------------------------------------------------------------
        // Email address of the customer who placed the order
        [BsonElement("customerEmail")]
        public string CustomerEmail { get; set; } = default!;

        // -------------------------------------------------------------
        // Order Contents
        // -------------------------------------------------------------
        // List of drink ObjectIds included in the order
        [BsonElement("drinkIds")]
        public List<string> DrinkIds { get; set; } = new();

        // Optional list of topping ObjectIds selected for the order
        [BsonElement("toppingIds")]
        public List<string>? ToppingIds { get; set; }

        // -------------------------------------------------------------
        // Pricing and Status
        // -------------------------------------------------------------
        // Total cost of the order (in AUD)
        [BsonElement("total")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Total { get; set; }

        // Current order status
        [BsonElement("status")]
        public string Status { get; set; } = "New";

        // -------------------------------------------------------------
        // MAGIC THREE DATES (Record Lifecycle Tracking)
        // -------------------------------------------------------------
        // CreatedUtc: Date/time when order was placed (UTC)
        [BsonElement("createdUtc")]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // UpdatedUtc: Date/time when order was last modified
        [BsonElement("updatedUtc")]
        public DateTime? UpdatedUtc { get; set; }

        // DeletedUtc: Soft delete marker (null means active order)
        [BsonElement("deletedUtc")]
        public DateTime? DeletedUtc { get; set; }
    }
}
