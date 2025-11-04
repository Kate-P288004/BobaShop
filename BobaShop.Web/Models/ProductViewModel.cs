// -----------------------------------------------------------------------------
// File: Models/ProductViewModels.cs
// Project: BobaShop.Web 
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
// Demonstrates the use of MVC ViewModels for UI data binding and separation
//   of concerns between controller logic and presentation layers.
// -----------------------------------------------------------------------------

using System.Collections.Generic;

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: ProductViewModel
    // Purpose:
    //   Represents a simplified summary of a drink product shown in the
    //   main menu view (Menu.cshtml). Contains only the fields required for
    //   listing drinks on the landing page.
    // -------------------------------------------------------------------------
    public class ProductViewModel
    {
        // Unique identifier for the product 
        public string Id { get; set; } = string.Empty;

        // Display name 
        public string Name { get; set; } = string.Empty;

        // Base price for display in the menu listing
        public decimal Price { get; set; }

        // Image URL path (relative to /wwwroot/images/)
        public string ImageUrl { get; set; } = string.Empty;

        // Short description 
        public string Description { get; set; } = string.Empty;
    }

    // -------------------------------------------------------------------------
    // Class: ToppingOption
    // Purpose:
    //   Represents a single topping that can be selected for a drink order.
    //   Used within the ProductDetailsViewModel to populate checkbox lists
    //   or selection menus in the product detail view.
    // -------------------------------------------------------------------------
    public class ToppingOption
    {
        // Short internal code used for identification 
        public string Code { get; set; } = string.Empty;

        // Display name for the UI 
        public string Name { get; set; } = string.Empty;

        // Price in AUD for adding this topping
        public decimal Price { get; set; }

        // Indicates whether the topping is selected in the UI
        public bool Selected { get; set; }
    }

    // -------------------------------------------------------------------------
    // Class: ProductDetailsViewModel
    // Purpose:
    //   Used for the product details page, representing one selected product
    //   and its configurable options. Allows users to choose size, sugar level,
    //   ice level, and toppings before adding to the shopping cart.
    //   Demonstrates complex model binding with lists and computed fields.
    // -------------------------------------------------------------------------
    public class ProductDetailsViewModel
    {
        // Product identity and display information
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // Base price (AUD) used for calculations
        public decimal BasePrice { get; set; }

        // Customisation options 
        public string Size { get; set; } = "S";   // e.g., S, M, L
        public int Sugar { get; set; } = 100;     // percentage (0–100)
        public int Ice { get; set; } = 100;       // percentage (0–100)

        // List of available topping options for the drink
        public List<ToppingOption> Toppings { get; set; } = new();

     
        public decimal CalculatedPrice => BasePrice;
    }
}
