// -----------------------------------------------------------------------------
// File: Controllers/ProductsController.cs
// Project: BobaShop.Web 
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Public controller for displaying drink products to all users (no login).
//   Integrates with the BobaShop.Api layer via IDrinksApi (HttpClient service).
//
//   Responsibilities:
//     - /Products/Menu     → list of all active drinks (menu page)
//     - /Products/Details  → individual drink info + toppings UI
//
//   Design notes:
//     • Uses dependency-injected IDrinksApi to call the API asynchronously.
//     • Razor views are strongly typed to ProductViewModel.
//     • Applies simple image fallback logic if the API returns no image URL.
//     • Static toppings list supports the Details page prototype UI.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BobaShop.Web.Models;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Controllers
{
    /// <summary>
    /// Presents the drinks catalogue (menu) and product detail pages.
    /// Public access; no authentication required.
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly IDrinksApi _api;

        // ---------------------------------------------------------------------
        // Constructor: injects the API abstraction for drinks retrieval.
        //   IDrinksApi defines GetAllAsync() and GetByIdAsync() using HttpClient.
        // ---------------------------------------------------------------------
        public ProductsController(IDrinksApi api) => _api = api;

        // =====================================================================
        // GET: /Products
        // Redirects to /Products/Menu to provide a clean entry point.
        // =====================================================================
        public IActionResult Index() => RedirectToAction(nameof(Menu));

        // =====================================================================
        // GET: /Products/Menu
        // Purpose:
        //   Loads all drinks from the API and renders the menu view.
        // View:
        //   Views/Products/Menu.cshtml (IEnumerable<ProductViewModel>)
        //
        // Error handling:
        //   - Displays a fallback message if API is unreachable (HttpRequestException).
        //   - Returns an empty list to avoid null reference issues in the Razor view.
        // =====================================================================
        public async Task<IActionResult> Menu()
        {
            try
            {
                var drinks = await _api.GetAllAsync();

                // Map raw DrinkVm data from API to simpler card view models.
                var list = drinks.Select(MapToCard).ToList();

                return View(list);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = "Could not connect to the API: " + ex.Message;
                return View(new List<ProductViewModel>());
            }
        }

        // =====================================================================
        // GET: /Products/Details/{id}
        // Purpose:
        //   Displays details for a single drink and its topping options.
        // View:
        //   Views/Products/Details.cshtml (ProductViewModel)
        //
        // Validation:
        //   - Verifies {id} format resembles Mongo ObjectId (24 hex chars).
        //   - Redirects to Menu if invalid.
        //
        // Behavior:
        //   - Fetches detailed drink info from the API.
        //   - Populates ProductViewModel including default toppings list.
        //   - Chooses image from API or falls back to a local file.
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            // Guard invalid IDs early to prevent unnecessary API calls.
            if (string.IsNullOrWhiteSpace(id) || id.Length != 24)
            {
                TempData["Alert"] = "That item is unavailable. Please choose a drink from the Menu.";
                return RedirectToAction(nameof(Menu));
            }

            try
            {
                var drink = await _api.GetByIdAsync(id);
                if (drink is null)
                    return NotFound();

                // Build a complete view model for the Details page.
                var model = new ProductViewModel
                {
                    Id = drink.Id,
                    Name = drink.Name ?? "Unnamed drink",
                    Description = drink.Description,
                    BasePrice = drink.BasePrice,
                    Price = drink.Price, // may mirror BasePrice, depending on API
                    SmallUpcharge = drink.SmallUpcharge,
                    MediumUpcharge = drink.MediumUpcharge,
                    LargeUpcharge = drink.LargeUpcharge,
                    DefaultSugar = drink.DefaultSugar,
                    DefaultIce = drink.DefaultIce,
                    IsActive = drink.IsActive,

                    // Use server image if available, else guess by drink name.
                    ImageUrl = string.IsNullOrWhiteSpace(drink.ImageUrl)
                        ? GuessImage(drink.Name)
                        : drink.ImageUrl,
                    ImageAlt = drink.ImageAlt ?? drink.Name,

                    // Prototype toppings list for selection UI on details view.
                    Toppings = new List<ToppingVm>
                    {
                        new ToppingVm { Name = "Pearls",        Price = 0.80m, IsActive = true },
                        new ToppingVm { Name = "Egg Pudding",   Price = 0.90m, IsActive = true },
                        new ToppingVm { Name = "Coconut Jelly", Price = 1.00m, IsActive = true }
                    }
                };

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                // Network-level or API unavailability.
                ViewBag.Error = "API request failed: " + ex.Message;
                return View("Error");
            }
        }

        // =====================================================================
        // Mapping helpers
        // ---------------------------------------------------------------------
        // Purpose:
        //   Converts a DrinkVm (from API) into ProductViewModel used by UI cards.
        //   Adds image fallback and base price normalization.
        // =====================================================================
        private static ProductViewModel MapToCard(DrinkVm d) => new()
        {
            Id = d.Id ?? string.Empty,
            Name = d.Name ?? "Unnamed drink",
            ImageUrl = string.IsNullOrWhiteSpace(d.ImageUrl)
                ? GuessImage(d.Name)
                : d.ImageUrl,
            ImageAlt = d.ImageAlt ?? d.Name,
            // Show price as API Price if present; else fallback to BasePrice.
            Price = d.Price != 0 ? d.Price : d.BasePrice
        };

        // =====================================================================
        // GuessImage
        // ---------------------------------------------------------------------
        // Purpose:
        //   Returns a reasonable default image path based on drink name keywords.
        //   Used when the API has no stored image.
        //
        // Logic:
        //   Simple substring checks for "taro", "matcha", "mango", etc.
        //   Defaults to classic milk tea if no keyword matches.
        // =====================================================================
        private static string GuessImage(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "/images/classic-milk-tea.jpg";

            var n = name.ToLowerInvariant();

            if (n.Contains("brown") && n.Contains("sugar")) return "/images/brown-sugar-boba.jpg";
            if (n.Contains("taro")) return "/images/taro-milk-tea.jpg";
            if (n.Contains("matcha") && n.Contains("strawberry")) return "/images/matcha-strawberry-latte.jpg";
            if (n.Contains("matcha")) return "/images/matcha-milk-tea.jpg";
            if (n.Contains("thai")) return "/images/thai-milk-tea.jpg";
            if (n.Contains("mango")) return "/images/mango-green-tea.jpg";
            if (n.Contains("passion")) return "/images/passionfruit-green-tea.jpg";
            if (n.Contains("oreo") || n.Contains("cocoa")) return "/images/oreo-cocoa-crush.jpg";

            return "/images/classic-milk-tea.jpg";
        }
    }
}
