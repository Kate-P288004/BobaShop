// -----------------------------------------------------------------------------
// File: Dtos/DrinkUpdateDto.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Defines the data transfer object (DTO) used when updating existing Drink
//   records via the API. It mirrors the structure of DrinkCreateDto but
//   includes an optional Id property for route binding and update tracking.
// -----------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace BobaShop.Api.Dtos
{
    // -------------------------------------------------------------------------
    // DTO: DrinkUpdateDto
    // Description:
    //   Used for PUT /api/v1/Drinks/{id} requests.
    //   Enables full replacement of an existing Drink document in MongoDB.
    // Notes:
    //   - Validation ensures consistent data integrity on updates.
    //   - The Id field is optional because it is normally supplied by the route.
    // -------------------------------------------------------------------------
    public class DrinkUpdateDto
    {
        // ---------------------------------------------------------------------
        // Optional internal identifier for the drink being updated.
        // - The controller overwrites this with the route parameter.
        // - Provided here only for deserialization compatibility.
        // ---------------------------------------------------------------------
        public string? Id { get; set; }

        // ---------------------------------------------------------------------
        // Display name of the drink (required).
        // - Same constraints as in DrinkCreateDto.
        // - Acts as a unique human-readable identifier in the UI.
        // ---------------------------------------------------------------------
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        // ---------------------------------------------------------------------
        // Optional long-form text for describing the drink.
        // - Typically shown in detailed menu views or tooltips.
        // - Limited to 400 characters.
        // ---------------------------------------------------------------------
        [StringLength(400)]
        public string? Description { get; set; }

        // ---------------------------------------------------------------------
        // Pricing configuration
        // - BasePrice is mandatory (zero allowed).
        // - Upcharges apply to size options.
        // - Each field is range-validated to prevent unrealistic values.
        // ---------------------------------------------------------------------
        [Range(0, 9999)] public decimal BasePrice { get; set; }
        [Range(0, 9999)] public decimal SmallUpcharge { get; set; }
        [Range(0, 9999)] public decimal MediumUpcharge { get; set; }
        [Range(0, 9999)] public decimal LargeUpcharge { get; set; }

        // ---------------------------------------------------------------------
        // Default modifiers for sugar and ice levels.
        // - Represent percentages used by UI sliders or order forms.
        // - Defaults remain at 50% for both.
        // ---------------------------------------------------------------------
        [Range(0, 100)] public int DefaultSugar { get; set; } = 50;
        [Range(0, 100)] public int DefaultIce { get; set; } = 50;

        // ---------------------------------------------------------------------
        // Status flag indicating whether the drink is available.
        // - Used by admins to hide seasonal or discontinued items.
        // ---------------------------------------------------------------------
        public bool IsActive { get; set; } = true;

        // ---------------------------------------------------------------------
        // Optional media data
        // - ImageUrl points to the drink’s thumbnail or hero image.
        // - ImageAlt provides accessibility text for screen readers.
        // ---------------------------------------------------------------------
        public string? ImageUrl { get; set; }
        public string? ImageAlt { get; set; }
    }
}
