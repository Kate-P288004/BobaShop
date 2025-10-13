using Microsoft.AspNetCore.Mvc;
using BobaShop.Web.Services;

namespace BobaShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApiService _api;
        public ProductsController(ApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var products = await _api.GetProductsAsync();
            return View(products);
        }
    }
}
