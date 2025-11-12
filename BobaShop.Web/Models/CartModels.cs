// -----------------------------------------------------------------------------
// File: Models/CartViewModel.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Represents the shopping cart session model used by BoBaTastic frontend.
//   Defines the data structure for cart items stored in session and used
//   throughout the ordering process (add/remove/update/view).
//
//   Key concepts demonstrated:
//     • Session management via a constant session key ("CART").
//     • Calculated properties (Subtotal, Total).
//     • Strongly-typed models for clean Razor page binding.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: CartKeys
    // Purpose:
    //   Central location for session-related key constants.
    //   Ensures consistent naming for HttpContext.Session usage.
    // -------------------------------------------------------------------------
    public static class CartKeys
    {
        public const string SessionKey = "CART"; // session key used for cart JSON
    }

    // -------------------------------------------------------------------------
    // Class: CartItem
    // Purpose:
    //   Represents a single drink or product entry inside the cart.
    //   Each item stores a product reference, size options, customization
    //   (sugar/ice levels, toppings), and calculated unit pricing.
    //
    // Notes:
    //   • UnitPrice already includes base price + size + toppings.
    //   • Subtotal = UnitPrice * Quantity.
    // -------------------------------------------------------------------------
    public class CartItem
    {
        // MongoDB drink ID or API reference
        public string ProductId { get; set; } = "";

        // Display name shown in cart and checkout
        public string Name { get; set; } = "";

        // Product image (used on summary/checkout pages)
        public string ImageUrl { get; set; } = "";

        // Drink size (S, M, or L)
        public string Size { get; set; } = "S";

        // Sugar percentage (0–100)
        public int Sugar { get; set; } = 100;

        // Ice percentage (0–100)
        public int Ice { get; set; } = 100;

        // Text summary of toppings: e.g. "Pearls(+0.80), Pudding(+1.00)"
        public string ToppingsSummary { get; set; } = "";

        // Price per drink, including upcharges and toppings
        public double UnitPrice { get; set; }

        // Number of units added to the cart
        public int Quantity { get; set; } = 1;

        // Derived property: total price for this line
        public double Subtotal => UnitPrice * Quantity;
    }

    // -------------------------------------------------------------------------
    // Class: CartViewModel
    // Purpose:
    //   Represents the entire shopping cart for a user session.
    //   Used by the CartController and Razor views for rendering totals.
    //
    // Structure:
    //   • Items: all current CartItem entries
    //   • Subtotal: sum of all item subtotals
    //   • Tax: optional placeholder for future extension
    //   • Total: grand total = Subtotal + Tax
    //
    // Example usage:
    //   var cart = HttpContext.Session.Get<CartViewModel>(CartKeys.SessionKey);
    // -------------------------------------------------------------------------
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();

        // Sum of all cart items
        public double Subtotal => Items.Sum(i => i.Subtotal);

        // Placeholder for tax or service fees
        public double Tax => 0;

        // Total with tax included
        public double Total => Subtotal + Tax;
    }
}
