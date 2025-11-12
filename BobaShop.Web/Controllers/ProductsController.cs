// -----------------------------------------------------------------------------
// File: Controllers/ProductsController.cs
// Project: BobaShop.Web 
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Public controller for drinks catalogue and product details pages.
//   - Calls BobaShop.Api via IDrinksApi (HttpClient).
//   - Strongly-typed Razor views (Menu: IEnumerable<ProductViewModel>, Details: ProductViewModel).
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
    /// Presents the drinks menu and details pages (no auth required).
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly IDrinksApi _api;

        public ProductsController(IDrinksApi api) => _api = api;

        // =====================================================================
        // GET: /Products  → redirect to /Products/Menu
        // =====================================================================
        public IActionResult Index() => RedirectToAction(nameof(Menu));

        // =====================================================================
        // GET: /Products/Menu
        // View: Views/Products/Menu.cshtml  (IEnumerable<ProductViewModel>)
        // =====================================================================
        public async Task<IActionResult> Menu()
        {
            try
            {
                var drinks = await _api.GetAllAsync();

                // Map API models to simple card view models
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
        // View: Views/Products/Details.cshtml  (ProductViewModel)
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            // Guard against bad ObjectId (24 hex chars)
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

                var model = new ProductViewModel
                {
                    Id = drink.Id,
                    Name = drink.Name ?? "Unnamed drink",
                    Description = drink.Description,
                    BasePrice = drink.BasePrice,
                    Price = drink.Price,           // if API uses BasePrice as selling price, that’s fine too
                    SmallUpcharge = drink.SmallUpcharge,
                    MediumUpcharge = drink.MediumUpcharge,
                    LargeUpcharge = drink.LargeUpcharge,
                    DefaultSugar = drink.DefaultSugar,
                    DefaultIce = drink.DefaultIce,
                    IsActive = drink.IsActive,
                    // Prefer API image if present, else fall back to a guessed local image
                    ImageUrl = string.IsNullOrWhiteSpace(drink.ImageUrl) ? GuessImage(drink.Name) : drink.ImageUrl,
                    ImageAlt = drink.ImageAlt ?? drink.Name,
                    // Static toppings list used by the Details page radio/checkbox UI
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
                ViewBag.Error = "API request failed: " + ex.Message;
                return View("Error");
            }
        }

        // =====================================================================
        // Mapping helpers
        // =====================================================================
        private static ProductViewModel MapToCard(DrinkVm d) => new()
        {
            Id = d.Id ?? string.Empty,
            Name = d.Name ?? "Unnamed drink",
            // Use API-provided image if set; else guess a local image path
            ImageUrl = string.IsNullOrWhiteSpace(d.ImageUrl) ? GuessImage(d.Name) : d.ImageUrl,
            ImageAlt = d.ImageAlt ?? d.Name,
            // Show a single price on the card (use BasePrice or Price per your API)
            Price = d.Price != 0 ? d.Price : d.BasePrice
        };

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
