// -----------------------------------------------------------------------------
// File: Models/ProductCreateVm.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Represents the ViewModel used by the Admin panel to create new drink items
//   in the BoBaTastic web application. This class is bound to the form in
//   /Areas/Admin/Views/Products/Create.cshtml and corresponds to the API’s
//   DrinkCreateDto model in BobaShop.Api.
//
//   Key Concepts Demonstrated:
//     • ASP.NET MVC model binding and Razor form data handling
//     • Strongly-typed ViewModels mapped to API DTOs
//     • UI-bound properties with optional image and customization fields
// -----------------------------------------------------------------------------

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: ProductCreateVm
    // Purpose:
    //   Used by the Admin dashboard to create new drink entries via the API.
    //   Defines all editable fields a manager can specify when adding a drink.
    //
    // Usage:
    //   - Bound to Create.cshtml form in the Admin area
    //   - Serialized to JSON and sent to POST /api/v1/Drinks
    // -------------------------------------------------------------------------
    public class ProductCreateVm
    {
        // ---------------------------------------------------------------------
        // Core product fields
        // ---------------------------------------------------------------------

        // Display name for the drink product
        public string Name { get; set; } = "";

        // Base price before adding size or toppings
        public decimal BasePrice { get; set; }

        // Size-based additional charges
        public decimal SmallUpcharge { get; set; }
        public decimal MediumUpcharge { get; set; }
        public decimal LargeUpcharge { get; set; }

        // Default sugar and ice levels (0–100%)
        public int DefaultSugar { get; set; } = 50;
        public int DefaultIce { get; set; } = 50;

        // Whether the drink is active and visible in the public menu
        public bool IsActive { get; set; } = true;

        // Optional short description for UI display
        public string? Description { get; set; }

        // ---------------------------------------------------------------------
        // Media fields
        // ---------------------------------------------------------------------

        // Image path or external URL shown on menu cards
        public string? ImageUrl { get; set; }

        // Alternative text for accessibility and SEO
        public string? ImageAlt { get; set; }
    }
}
