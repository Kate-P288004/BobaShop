// -----------------------------------------------------------------------------
// File: Controllers/ProductsController.cs
// Project: BobaShop.Web 
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Presents the public product catalogue (menu) and product details pages.
//   Uses an in-memory list of drinks for the web UI demo. Each product is
//   represented by a ProductViewModel and image under /wwwroot/images.
//   Demonstrates MVC routing, model binding to views, and clean controller
//   actions returning strongly-typed view models.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using BobaShop.Web.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace BobaShop.Web.Controllers
{
    // -------------------------------------------------------------------------
    // Controller: ProductsController
    // Purpose:
    //   Public endpoints for browsing the drinks menu and viewing item details.
    //   This controller does not require authentication (landing experience).
    // Mapping: ICTPRG556 Controllers, views, and routing
    // -------------------------------------------------------------------------
    public class ProductsController : Controller
    {
        // ---------------------------------------------------------------------
        // In-memory menu for demo purposes
        // Notes:
        //   IDs are simple strings "1".."10" to keep the UI stable.
        //   ImageUrl points to /wwwroot/images/*.jpg.
        //   Price is used as the product's base price on the details page.
        //   Keep this data aligned with API seed data for consistent UX.
        // ---------------------------------------------------------------------
        private static readonly List<ProductViewModel> Drinks = new()
        {
            new() { Id = "1",  Name = "Classic Milk Tea",            Price = 6.50m, ImageUrl = "/images/classic-milk-tea.jpg" },
            new() { Id = "2",  Name = "Brown Sugar Boba",            Price = 7.00m, ImageUrl = "/images/brown-sugar-boba.jpg" },
            new() { Id = "3",  Name = "Taro Milk Tea",               Price = 6.80m, ImageUrl = "/images/taro-milk-tea.jpg" },
            new() { Id = "4",  Name = "Matcha Milk Tea",             Price = 7.00m, ImageUrl = "/images/matcha-milk-tea.jpg" },
            new() { Id = "5",  Name = "Thai Milk Tea",               Price = 6.80m, ImageUrl = "/images/thai-milk-tea.jpg" },
            new() { Id = "6",  Name = "Mango Green Tea",             Price = 6.20m, ImageUrl = "/images/mango-green-tea.jpg" },
            new() { Id = "7",  Name = "Passionfruit Green Tea",      Price = 6.20m, ImageUrl = "/images/passionfruit-green-tea.jpg" },
            new() { Id = "8",  Name = "Dirty Brown Sugar Cream Cap", Price = 7.50m, ImageUrl = "/images/dirty-brown-sugar-creamcap.jpg" },
            new() { Id = "9",  Name = "Matcha Strawberry Latte",     Price = 7.80m, ImageUrl = "/images/matcha-strawberry-latte.jpg" },
            new() { Id = "10", Name = "Oreo Cocoa Crush",            Price = 7.50m, ImageUrl = "/images/oreo-cocoa-crush.jpg" }
        };

        // =====================================================================
        // GET: /Products
        // Purpose:
        //   Friendly top-level route that redirects to the actual menu action.
        //   Keeps URLs intuitive while allowing the menu view to live at /Menu.
        // =====================================================================
        public IActionResult Index() => RedirectToAction(nameof(Menu));

        // =====================================================================
        // GET: /Products/Menu
        // Purpose:
        //   Displays the full drinks list. The view enumerates the list and
        //   renders product cards linking to /Products/Details/{id}.
        // View:
        //   Views/Products/Menu.cshtml (strongly-typed: IEnumerable<ProductViewModel>)
        // =====================================================================
        public IActionResult Menu() => View(Drinks);

        // =====================================================================
        // GET: /Products/Details/{id}
        // Purpose:
        //   Shows a single product with base price, hero image, and available
        //   topping options. The returned ProductDetailsViewModel is used by
        //   the view to post a Cart item (size/sugar/ice/toppings selection).
        // View:
        //   Views/Products/Details.cshtml 
        // =====================================================================
        [HttpGet]
        public IActionResult Details(string id)
        {
            var p = Drinks.FirstOrDefault(d => d.Id == id);
            if (p is null) return NotFound();

            var model = new ProductDetailsViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                BasePrice = p.Price,

                // Default set of toppings for the UI selector.
                
                Toppings = new()
                {
                    new ToppingOption { Code = "pearls",  Name = "Pearls",        Price = 0.80m },
                    new ToppingOption { Code = "pudding", Name = "Egg Pudding",   Price = 0.90m },
                    new ToppingOption { Code = "coconut", Name = "Coconut Jelly", Price = 1.00m }
                }
            };

            return View(model);
        }
    }
}
