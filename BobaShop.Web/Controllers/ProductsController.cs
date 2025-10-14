using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BobaShop.Web.Services;

namespace BobaShop.Web.Controllers
{
    [Authorize] // require login for all actions in this controller
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
