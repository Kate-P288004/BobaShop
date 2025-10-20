// ---------------------------------------------------------------
// File: Controllers/CartController.cs
// Student: Kate Odabas (P288004)
// Project: BoBaTastic – Web
// Purpose: Session-backed cart actions (protected for signed-in users)
// ---------------------------------------------------------------
using BobaShop.Web.Models;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Controllers
{
    // Require login (Customer or Admin) for all cart endpoints
    [Authorize(Roles = "Customer,Admin")]
    public class CartController : Controller
    {
        private readonly ICartService _cart;
        public CartController(ICartService cart) => _cart = cart;

        // -----------------------------------------------------------
        // GET: /Cart
        // Show current cart
        // -----------------------------------------------------------
        [HttpGet]
        public IActionResult Index()
        {
            var vm = _cart.GetCart();
            return View(vm);
        }

        // -----------------------------------------------------------
        // POST: /Cart/Add
        // Add an item (from product details form)
        // -----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add([FromForm] CartItem item)
        {
            if (item is null)
            {
                TempData["CartMessage"] = "Invalid item.";
                return RedirectToAction(nameof(Index));
            }

            // Basic guards
            item.Quantity = Math.Max(1, item.Quantity);

            // TODO (later): compute UnitPrice server-side from product + options instead of trusting UI
            _cart.Add(item);

            TempData["CartMessage"] = "Item added to cart.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------
        // POST: /Cart/UpdateQty
        // Update quantity for a specific customization combo
        // -----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQty(string productId, string size, int sugar, int ice, string toppingsSummary, int qty)
        {
            qty = Math.Max(1, qty);

            _cart.UpdateQty(productId, size, sugar, ice, toppingsSummary, qty);
            TempData["CartMessage"] = "Quantity updated.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------
        // POST: /Cart/Remove
        // Remove a specific customization combo
        // -----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string productId, string size, int sugar, int ice, string toppingsSummary)
        {
            _cart.Remove(productId, size, sugar, ice, toppingsSummary);
            TempData["CartMessage"] = "Item removed.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------
        // POST: /Cart/Clear
        // Clear the whole cart
        // -----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.Clear();
            TempData["CartMessage"] = "Cart cleared.";
            return RedirectToAction(nameof(Index));
        }
    }
}
