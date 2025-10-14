// ---------------------------------------------------------------
// File: ProductsController.cs
// Step 3: Details + Customize (placeholder data)
// ---------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;
using BobaShop.Web.Models;

namespace BobaShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        // In-memory placeholder list (replace with API later)
        private static readonly List<ProductViewModel> _products = new()
        {
            new ProductViewModel { Id = "1", Name = "Classic Milk Tea", Price = 6.50, ImageUrl = "/images/classic-milk-tea.jpg" },
            new ProductViewModel { Id = "2", Name = "Brown Sugar Boba", Price = 7.00, ImageUrl = "/images/brown-sugar-boba.jpg" },
            new ProductViewModel { Id = "3", Name = "Taro Milk Tea", Price = 6.80, ImageUrl = "/images/taro-milk-tea.jpg" },
            new ProductViewModel { Id = "4", Name = "Mango Green Tea", Price = 6.20, ImageUrl = "/images/mango-green-tea.jpg" }
        };

        public IActionResult Index() => View(_products);

        [HttpGet]
        public IActionResult Details(string id)
        {
            var p = _products.FirstOrDefault(x => x.Id == id);
            if (p is null) return NotFound();

            var vm = new ProductDetailsViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                BasePrice = p.Price,
                Toppings = new List<ToppingOption>
                {
                    new() { Code="pearls", Name="Tapioca Pearls", Price=0.80 },
                    new() { Code="pudding", Name="Egg Pudding",   Price=1.00 },
                    new() { Code="grass",   Name="Grass Jelly",   Price=0.90 },
                    new() { Code="lychee",  Name="Lychee Jelly",  Price=0.90 }
                }
            };
            return View(vm);
        }
    }
}
