// -----------------------------------------------------------------------------
// File: Models/DrinkVm.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Represents a drink data transfer model (ViewModel) used across both the
//   public and admin sections of the BoBaTastic web application.
//
//   This class mirrors the API’s Drink document structure in MongoDB while
//   remaining tailored for the web layer. It’s primarily used to display,
//   edit, or manage drink information retrieved from BobaShop.Api.
//
//   Key Concepts Demonstrated:
//     • Strongly-typed ViewModel pattern between MVC and Web API
//     • Nullable properties for optional fields
//     • Lifecycle tracking via “Magic Three Dates” (Created/Updated/Deleted)
// -----------------------------------------------------------------------------

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: DrinkVm
    // Purpose:
    //   A flexible model for representing drinks retrieved from or sent to the API.
    //   Used by:
    //     • Public website (ProductsController)
    //     • Admin dashboard (ProductsController under /Admin area)
    //   Supports binding to Razor views for list, detail, and CRUD pages.
    // -------------------------------------------------------------------------
    public class DrinkVm
    {
        // Unique MongoDB document identifier (ObjectId string)
        public string? Id { get; set; }

        // Human-readable product name (required)
        public string Name { get; set; } = string.Empty;

        // Optional short description of ingredients or flavor
        public string? Description { get; set; }

        // Base cost (before size or toppings); used in pricing calculations
        public decimal BasePrice { get; set; } = 0m;

        // Current effective selling price
        public decimal Price { get; set; } = 0m;

        // Size-based surcharges (optional)
        public decimal SmallUpcharge { get; set; } = 0m;
        public decimal MediumUpcharge { get; set; } = 0m;
        public decimal LargeUpcharge { get; set; } = 0m;

        // Default customization values for sugar and ice levels (0–100%)
        public int DefaultSugar { get; set; } = 50;
        public int DefaultIce { get; set; } = 50;

        // Whether the drink is currently active and visible in menus
        public bool IsActive { get; set; } = true;

        // Optional image path or external URL
        public string? ImageUrl { get; set; }

        // Alternate text for accessibility and SEO
        public string? ImageAlt { get; set; }

        // ---------------------------------------------------------------------
        // Metadata fields (Magic Three Dates pattern)
        // Used for admin dashboards and auditing in MongoDB-based systems.
        // ---------------------------------------------------------------------

        // Timestamp when the record was created (UTC)
        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;

        // Timestamp when the record was last modified (UTC)
        public DateTime? UpdatedUtc { get; set; }

        // Timestamp when the record was soft-deleted (null = active)
        public DateTime? DeletedUtc { get; set; }
    }
}
