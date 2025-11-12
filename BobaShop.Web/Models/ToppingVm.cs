// -----------------------------------------------------------------------------
// File: Models/ToppingVm.cs
// Project: BobaShop.Web (BoBatastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Represents a topping model shared between Admin and Public views.
//   Mirrors the structure of the Topping document in MongoDB (via API).
//   Used for displaying available toppings, managing CRUD operations in
//   the Admin area, and rendering topping options on drink detail pages.
//
//   Key Concepts Demonstrated:
//     • Strongly typed ViewModel for MVC data binding
//     • Compatibility with MongoDB document structure
//     • Computed property (Code) for Razor element binding
//     • Lifecycle tracking via “Magic Three Dates” pattern
// -----------------------------------------------------------------------------

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: ToppingVm
    // Purpose:
    //   Represents a drink topping within the BoBaTastic system.
    //   Used across Admin CRUD pages and customer-facing drink detail views.
    //
    // Usage:
    //   - Admin area → Manage toppings (Create, Edit, Delete)
    //   - Public site → Display topping selection options
    // -------------------------------------------------------------------------
    public class ToppingVm
    {
        // Unique identifier (maps to MongoDB ObjectId)
        public string? Id { get; set; }

        // Display name (e.g., “Pearls”, “Coconut Jelly”)
        public string Name { get; set; } = string.Empty;

        // Price adjustment added to the base drink price
        public decimal Price { get; set; }

        // Whether the topping is available for selection
        public bool IsActive { get; set; } = true;

        // ---------------------------------------------------------------------
        // Lifecycle metadata (“Magic Three Dates”)
        // Used for auditing and Admin dashboard timestamps.
        // ---------------------------------------------------------------------
        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }
        public DateTime? DeletedUtc { get; set; }

        // ---------------------------------------------------------------------
        // Computed property
        // Purpose:
        //   Generates a lowercase slug for safe use as HTML element IDs
        //   (e.g., "Egg Pudding" → "egg-pudding").
        // ---------------------------------------------------------------------
        public string Code =>
            (Name ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", "-");
    }
}
