// BobaShop.Web/Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using BobaShop.Web.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace BobaShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        // In-memory menu (IDs are "1".."10"; images in /images/*.jpg)
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

        // /Products  -> /Products/Menu
        public IActionResult Index() => RedirectToAction(nameof(Menu));

        // /Products/Menu
        public IActionResult Menu() => View(Drinks);

        // /Products/Details/{id}  (id is "1".."10")
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
                Toppings = new()
                {
                    new ToppingOption { Code="pearls",  Name="Pearls",        Price=0.80m },
                    new ToppingOption { Code="pudding", Name="Egg Pudding",   Price=0.90m },
                    new ToppingOption { Code="coconut", Name="Coconut Jelly", Price=1.00m }
                }
            };
            return View(model); // Views/Products/Details.cshtml
        }
    }
}
