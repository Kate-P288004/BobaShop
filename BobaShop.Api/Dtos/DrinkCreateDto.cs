// -----------------------------------------------------------------------------
// File: Dtos/DrinkCreateDto.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Defines the data transfer object (DTO) used when creating new Drink records
//   via the API. This model includes validation attributes for server-side
//   validation and helps ensure consistent data integrity between client and
//   MongoDB models.
// -----------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace BobaShop.Api.Dtos
{
    // -------------------------------------------------------------------------
    // DTO: DrinkCreateDto
    // Description:
    //   Represents the incoming payload for POST /api/v1/Drinks requests.
    //   Ensures user input is validated before being mapped to the Drink model.
    // Usage:
    //   - Bound automatically by ASP.NET Core Model Binding.
    //   - Validation attributes trigger automatic 400 Bad Request responses
    //     when constraints are violated.
    // -------------------------------------------------------------------------
    public class DrinkCreateDto
    {
        // ---------------------------------------------------------------------
        // Name of the drink (required)
        // - Used for display and search in the frontend.
        // - Limited to 80 characters for UI consistency and storage efficiency.
        // ---------------------------------------------------------------------
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        // ---------------------------------------------------------------------
        // Optional description for the drink.
        // - Provides short text for menus or detailed pages.
        // - Maximum length of 400 characters to avoid overflow in listings.
        // ---------------------------------------------------------------------
        [StringLength(400)]
        public string? Description { get; set; }

        // ---------------------------------------------------------------------
        // Base price of the drink (AUD).
        // - Zero allowed to accommodate promotions or free drinks.
        // - Decimal precision managed by MongoDB’s Decimal128 mapping.
        // ---------------------------------------------------------------------
        [Range(0, 9999)] // zero allowed
        public decimal BasePrice { get; set; }

        // ---------------------------------------------------------------------
        // Price adjustments by size.
        // - Small, Medium, and Large upcharges define how much to add to base.
        // - Used to calculate final order totals.
        // ---------------------------------------------------------------------
        [Range(0, 9999)]
        public decimal SmallUpcharge { get; set; }

        [Range(0, 9999)]
        public decimal MediumUpcharge { get; set; }

        [Range(0, 9999)]
        public decimal LargeUpcharge { get; set; }

        // ---------------------------------------------------------------------
        // Default customization values (percentages)
        // - Represent the initial sugar and ice level shown in UI sliders.
        // - Range 0–100; defaults to 50% for both fields.
        // ---------------------------------------------------------------------
        [Range(0, 100)]
        public int DefaultSugar { get; set; } = 50;

        [Range(0, 100)]
        public int DefaultIce { get; set; } = 50;

        // ---------------------------------------------------------------------
        // Flag indicating whether the drink is available for ordering.
        // - Set to true by default to ensure new items appear on the menu.
        // ---------------------------------------------------------------------
        public bool IsActive { get; set; } = true;

        // ---------------------------------------------------------------------
        // Optional image metadata for the drink.
        // - ImageUrl: path to an image file (stored in /wwwroot/images/drinks/)
        // - ImageAlt: alternative text for accessibility and SEO.
        // ---------------------------------------------------------------------
        public string? ImageUrl { get; set; }
        public string? ImageAlt { get; set; }
    }
}
