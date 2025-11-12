// -----------------------------------------------------------------------------
// File: Models/Customer.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Represents a registered customer document stored in the MongoDB database.
//   Includes user profile information, authentication hash, loyalty points,
//   and lifecycle tracking fields using the “Magic Three Dates” pattern
//   (CreatedAt, UpdatedAt, DeletedAt) to support creation, updates,
//   and soft deletion for audit-friendly persistence.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BobaShop.Api.Models
{
    // -------------------------------------------------------------------------
    // Model: Customer
    // Purpose:
    //   Stores essential account and loyalty information for customers
    //   interacting with the BobaShop application. 
    //   Demonstrates MongoDB document mapping using BSON attributes.
    // Notes:
    //   - The “Magic Three Dates” pattern tracks document state transitions.
    //   - This model supports both authentication and reward tracking use cases.
    // Mapping: ICTPRG554 PE1.1 / PE1.2 / ICTPRG556 PE2.1
    // -------------------------------------------------------------------------
    public class Customer
    {
        // ---------------------------------------------------------------------
        // Unique MongoDB ObjectId used as the primary key.
        // - Automatically generated when inserted into MongoDB.
        // - Stored as a string for JSON serialization compatibility.
        // ---------------------------------------------------------------------
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        // ---------------------------------------------------------------------
        // Customer's full display name.
        // - Shown on profile pages and order receipts.
        // - Typically entered during registration or updated via profile edit.
        // ---------------------------------------------------------------------
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        // ---------------------------------------------------------------------
        // Customer’s registered email address.
        // - Serves as the unique login identifier for authentication.
        // - Also used for sending order confirmations and receipts.
        // ---------------------------------------------------------------------
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        // ---------------------------------------------------------------------
        // Password hash stored securely for authentication.
        // - Plain-text passwords are never persisted.
        // - Hash generated via ASP.NET Identity’s password hasher.
        // ---------------------------------------------------------------------
        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        // ---------------------------------------------------------------------
        // Loyalty points accumulated through purchases.
        // - Typically: 1 point = $1 spent.
        // - Used for reward redemption and promotions.
        // ---------------------------------------------------------------------
        [BsonElement("points")]
        public int Points { get; set; }

        // ---------------------------------------------------------------------
        // Record lifecycle tracking — “Magic Three Dates” pattern
        // Purpose:
        //   Tracks when the document was created, last updated, or soft-deleted.
        //   This pattern is used across multiple collections for consistency.
        // ---------------------------------------------------------------------

        // Date and time when the record was created (UTC).
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Date and time of the most recent modification (UTC).
        // - Null means the record has not been updated since creation.
        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Soft deletion marker (UTC).
        // - Null = active record.
        // - Non-null = record hidden but preserved for audit/recovery.
        [BsonElement("deletedAt")]
        public DateTime? DeletedAt { get; set; }
    }
}
