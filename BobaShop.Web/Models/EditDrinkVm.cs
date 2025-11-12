// -----------------------------------------------------------------------------
// File: Models/EditDrinkVm.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Represents the ViewModel used by the Admin panel for editing drink products.
//   Used in the “Edit” and “Create” forms under /Areas/Admin/Views/Products/.
//   Includes validation attributes for form inputs and provides a clean data
//   structure to send updates to the BobaShop.Api endpoints.
//
//   Key Concepts Demonstrated:
//     • ASP.NET MVC Model Binding and Validation (DataAnnotations)
//     • Strong typing between Razor forms and API DTOs
//     • Field-level constraints and defaults for form safety
// -----------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: EditDrinkVm
    // Purpose:
    //   A form-bound model used to create or update drink records via
    //   the Admin dashboard. Validation attributes ensure consistent input
    //   formatting and enforce limits that align with API-side DTO rules.
    //
    // Usage:
    //   - Bound to Razor views: Create.cshtml / Edit.cshtml
    //   - Passed to the Admin ProductsController (POST actions)
    // -------------------------------------------------------------------------
    public class EditDrinkVm
    {
        // Display name for the drink product (required)
        [Required, StringLength(60)]
        public string Name { get; set; } = "";

        // Base cost before size or topping adjustments
        [Range(0, 1000)]
        public decimal BasePrice { get; set; }

        // Optional product description shown in listings
        [StringLength(200)]
        public string? Description { get; set; }

        // Upcharges for different drink sizes
        [Range(0, 10)]
        public decimal SmallUpcharge { get; set; }

        [Range(0, 10)]
        public decimal MediumUpcharge { get; set; }

        [Range(0, 10)]
        public decimal LargeUpcharge { get; set; }

        // Default sugar and ice settings (percentages, 0–100)
        [Range(0, 100)]
        public int DefaultSugar { get; set; } = 50;

        [Range(0, 100)]
        public int DefaultIce { get; set; } = 50;

        // Controls product visibility in the customer-facing menu
        public bool IsActive { get; set; } = true;
    }
}
