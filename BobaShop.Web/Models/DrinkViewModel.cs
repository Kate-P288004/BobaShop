// -----------------------------------------------------------------------------
// File: Models/DrinkViewModel.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Represents a simplified drink data model used on the BoBaTastic web frontend.
//   This view model is primarily designed for presentation in Razor views such as
//   product cards, menus, or featured sections.
//
//   Key Concepts Demonstrated:
//     • ViewModel pattern — decouples UI data from API/database models.
//     • Typed Razor binding for Drink lists and detail pages.
//     • Image, description, and pricing attributes tailored for UI display.
// -----------------------------------------------------------------------------

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: DrinkViewModel
    // Purpose:
    //   A presentation model for displaying drink products in the web interface.
    //   Typically populated by mapping from API DTOs (DrinkVm) or MongoDB models.
    //
    // Usage:
    //   - Used in /Views/Products/Menu.cshtml to display drink cards.
    //   - Used in /Views/Products/Details.cshtml for detailed drink info.
    // -------------------------------------------------------------------------
    public class DrinkViewModel
    {
        // Unique identifier (MongoDB ObjectId, GUID, or slug for routing)
        public string Id { get; set; } = string.Empty;

        // Display name shown on menu and product cards
        public string Name { get; set; } = string.Empty;

        // Short text description of the drink
        public string Description { get; set; } = string.Empty;

        // Price in AUD; stored as decimal to preserve precision for currency
        public decimal Price { get; set; }

        // Image path or URL shown in product listings
        public string ImageUrl { get; set; } = string.Empty;

        // Marks drink as “featured” or “popular” on the homepage or carousel
        public bool IsPopular { get; set; }
    }
}
