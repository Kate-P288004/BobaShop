using BobaShop.Web.Models;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cart;
        public CartController(ICartService cart) => _cart = cart;

        [HttpGet]
        public IActionResult Index()
        {
            var vm = _cart.GetCart();
            return View(vm);
        }

        // Simple form post from Details page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(CartItem item)
        {
            // Server trusts the UnitPrice sent by UI for now (later compute on server).
            _cart.Add(item);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQty(string productId, string size, int sugar, int ice, string toppingsSummary, int qty)
        {
            _cart.UpdateQty(productId, size, sugar, ice, toppingsSummary, qty);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string productId, string size, int sugar, int ice, string toppingsSummary)
        {
            _cart.Remove(productId, size, sugar, ice, toppingsSummary);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.Clear();
            return RedirectToAction("Index");
        }
    }
}
