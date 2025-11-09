// -----------------------------------------------------------------------------
// File: Controllers/ProductsController.cs
// Project: BobaShop.Web 
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Controller responsible for displaying the drinks catalogue and product 
//   details pages in the BoBaTastic web app.
//
//   - Calls BobaShop.Api through the IDrinksApi service (HttpClient).
//   - Fetches real MongoDB data (no in-memory demo list).
//   - Demonstrates MVC routing, dependency injection, async/await,
//     and strongly-typed Razor views.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using BobaShop.Web.Models;
using BobaShop.Web.Services;
using System.Linq;
using System.Net.Http;

namespace BobaShop.Web.Controllers
{
    /// <summary>
    /// Public controller that presents the drinks menu and details pages.
    /// Fully accessible without authentication.
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly IDrinksApi _api;

        /// <summary>
        /// Inject the API client used to retrieve drinks.
        /// </summary>
        public ProductsController(IDrinksApi api) => _api = api;

        // =====================================================================
        // GET: /Products
        // Friendly entry that redirects to /Products/Menu.
        // =====================================================================
        public IActionResult Index() => RedirectToAction(nameof(Menu));

        // =====================================================================
        // GET: /Products/Menu
        // Fetch all drinks from the API and render cards on the Menu page.
        // View: Views/Products/Menu.cshtml (IEnumerable<ProductViewModel>)
        // =====================================================================
        public async Task<IActionResult> Menu()
        {
            try
            {
                var drinks = await _api.GetAllAsync();

                // Explicit lambda avoids Select(...) overload ambiguity.
                var list = drinks.Select(d => MapToCard(d)).ToList();

                return View(list);
            }
            catch (HttpRequestException ex)
            {
                // Friendly message if the API is unreachable.
                ViewBag.Error = "Could not connect to the API: " + ex.Message;

                // Optional: show a custom page if you’ve created Views/Products/ApiError.cshtml
                // return View("ApiError");

                return View(new List<ProductViewModel>());
            }
        }

        // =====================================================================
        // GET: /Products/Details/{id}
        // Load one drink by its MongoDB ObjectId and build a details view model.
        // View: Views/Products/Details.cshtml (ProductDetailsViewModel)
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            // Guard: old demo links like /Products/Details/1
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

                var model = new ProductDetailsViewModel
                {
                    Id = drink.Id!,
                    Name = drink.Name ?? "Unnamed drink",
                    ImageUrl = GuessImage(drink.Name),
                    BasePrice = drink.BasePrice,

                    // Static toppings for the customisation selector.
                    Toppings = new()
                    {
                        new ToppingOption { Code = "pearls",  Name = "Pearls",        Price = 0.80m },
                        new ToppingOption { Code = "pudding", Name = "Egg Pudding",   Price = 0.90m },
                        new ToppingOption { Code = "coconut", Name = "Coconut Jelly", Price = 1.00m }
                    }
                };

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = "API request failed: " + ex.Message;

                // Optional custom page; otherwise Shared/Error.cshtml will be used if you return View("Error")
                // return View("ApiError");

                return View("Error");
            }
        }

        // =====================================================================
        // Helper: Map API drink (DrinkVm) to the card model used by Menu.cshtml
        // =====================================================================
        private static ProductViewModel MapToCard(DrinkVm d) => new()
        {
            Id = d.Id ?? string.Empty,
            Name = d.Name ?? "Unnamed drink",
            Price = d.BasePrice,
            ImageUrl = GuessImage(d.Name)
        };

        // =====================================================================
        // Helper: Guess an image path based on drink name to match /wwwroot/images
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
