// ---------------------------------------------------------------
// File: ProductsController.cs
// Student: Kate Odabas (P288004)
// Project: Boba Shop – Product Catalog Page
// Description: Displays product list and later connects to API.
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BobaShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            // Placeholder data — later this will come from the API (GET /api/v1/products)
            var products = new List<ProductViewModel>
            {
                new ProductViewModel { Id = "1", Name = "Classic Milk Tea", Price = 6.50, ImageUrl = "/images/classic-milk-tea.jpg" },
    new ProductViewModel { Id = "2", Name = "Brown Sugar Boba", Price = 7.00, ImageUrl = "/images/brown-sugar-boba.jpg" },
    new ProductViewModel { Id = "3", Name = "Taro Milk Tea", Price = 6.80, ImageUrl = "/images/taro-milk-tea.jpg" },
    new ProductViewModel { Id = "4", Name = "Mango Green Tea", Price = 6.20, ImageUrl = "/images/mango-green-tea.jpg" }
            };

            return View(products);
        }
    }

    // simple view model
    public class ProductViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
