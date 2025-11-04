// -----------------------------------------------------------------------------
// File: Customer.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Represents a registered customer stored in the MongoDB database.
//   Includes lifecycle tracking fields (CreatedAt, UpdatedAt, DeletedAt)
//   to demonstrate use of the “Magic Three Dates” pattern for record
//   creation, modification, and soft deletion.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BobaShop.Api.Models
{
    // -------------------------------------------------------------------------
    // Model: Customer
    // Purpose:
    //   Stores user profile and loyalty data for authenticated customers.
    //   Used across registration, login, and reward tracking features.
    //   Demonstrates use of MongoDB BSON mapping attributes.
    // Mapping: ICTPRG554 PE1.1 / ICTPRG554 PE1.2 / ICTPRG556 PE2.1
    // -------------------------------------------------------------------------
    public class Customer
    {
        // Unique identifier (MongoDB ObjectId)
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        // Full name of the customer (displayed on account page)
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        // Email address used for login and order tracking
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        // Hashed password for authentication (not stored in plain text)
        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        // Loyalty points earned (1 point = $1 spent)
        [BsonElement("points")]
        public int Points { get; set; }

        // -------------------------------------------------------------
        // Record lifecycle tracking fields (Magic Three Dates pattern)
        // -------------------------------------------------------------

        // Date the customer record was created (UTC)
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Date the customer record was last updated (UTC)
        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Date the customer record was soft-deleted (null = active)
        [BsonElement("deletedAt")]
        public DateTime? DeletedAt { get; set; }
    }
}
