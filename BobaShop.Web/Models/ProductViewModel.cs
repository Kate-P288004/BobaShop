// -----------------------------------------------------------------------------
// File: Models/ProductViewModel.cs
// Project: BobaShop.Web (BoBatastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Represents a unified product model used for displaying drink details and
//   menu listings in the BoBaTastic web interface. Also supports binding in
//   the Admin area for editing and auditing product information.
//
//   This model combines key properties from the BobaShop.Api Drink document,
//   including pricing, customization, lifecycle metadata, and image fields.
//   It also provides a Toppings list for dynamic UI generation on the Details
//   page.
//
//   Key Concepts Demonstrated:
//     • MVC ViewModel pattern for clean separation of web and API layers
//     • DataAnnotations for validation and UI constraints
//     • Integration of “Magic Three Dates” lifecycle tracking
// -----------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: ProductViewModel
    // Purpose:
    //   Provides a unified model for both public and admin Razor views:
    //     • Public site → Product Details and Menu pages
    //     • Admin site  → Product listing and edit pages
    //   Designed for compatibility with BobaShop.Api Drink entities.
    // -------------------------------------------------------------------------
    public class ProductViewModel
    {
        // ---------------------------------------------------------------------
        // Identity and relations
        // ---------------------------------------------------------------------

        // Unique identifier (matches MongoDB _id from API)
        public string? Id { get; set; }

        // List of available toppings for UI rendering in the Details view
        public List<ToppingVm> Toppings { get; set; } = new();

        // ---------------------------------------------------------------------
        // Product basics
        // ---------------------------------------------------------------------

        // Display name for the drink (required)
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        // Optional flavor or product description
        [StringLength(200)]
        public string? Description { get; set; }

        // ---------------------------------------------------------------------
        // Pricing
        // ---------------------------------------------------------------------

        // Base cost (default price before add-ons or size changes)
        [Range(0, 1000)]
        public decimal BasePrice { get; set; } = 0m;

        // Effective selling price (may include adjustments)
        [Range(0, 1000)]
        public decimal Price { get; set; } = 0m;

        // Size-based price increments
        [Range(0, 1000)]
        public decimal SmallUpcharge { get; set; } = 0m;

        [Range(0, 1000)]
        public decimal MediumUpcharge { get; set; } = 0m;

        [Range(0, 1000)]
        public decimal LargeUpcharge { get; set; } = 0m;

        // ---------------------------------------------------------------------
        // Defaults and customization options
        // ---------------------------------------------------------------------

        // Default sugar level (0–100%)
        [Range(0, 100)]
        public int DefaultSugar { get; set; } = 50;

        // Default ice level (0–100%)
        [Range(0, 100)]
        public int DefaultIce { get; set; } = 50;

        // Determines if the product is currently active and visible
        public bool IsActive { get; set; } = true;

        // ---------------------------------------------------------------------
        // Media
        // ---------------------------------------------------------------------

        // Optional URL or path to the product image
        public string? ImageUrl { get; set; }

        // Alternative image text for accessibility
        public string? ImageAlt { get; set; }

        // ---------------------------------------------------------------------
        // Lifecycle metadata (“Magic Three Dates”)
        // Used in Admin dashboards for tracking changes and deletions.
        // ---------------------------------------------------------------------

        // Creation timestamp (UTC)
        public DateTime? CreatedUtc { get; set; }

        // Last update timestamp (UTC)
        public DateTime? UpdatedUtc { get; set; }

        // Soft-deletion timestamp (null = active)
        public DateTime? DeletedUtc { get; set; }
    }
}
