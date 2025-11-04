// -----------------------------------------------------------------------------
// File: Controllers/CartController.cs
// Project: BobaShop.Web (BoBaTastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Handles all shopping cart actions for signed-in users (Customers and Admins).
//   Cart data is maintained in session storage using a custom ICartService.
//   Implements full CRUD operations for cart items: add, update, remove, clear.
//   Demonstrates MVC model binding, session management, TempData messaging,
//   and secure POST handling with Anti-Forgery validation.
// -----------------------------------------------------------------------------

using BobaShop.Web.Models;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Controllers
{
    // -------------------------------------------------------------------------
    // Controller: CartController
    // Purpose:
    //   Provides authenticated users with a shopping cart interface.
    //   Uses dependency injection to access ICartService for session-based
    //   data persistence and state management.
    //   Protects all actions with [Authorize] to ensure secure access.
    // Mapping: ICTPRG556  MVC routes, model binding, session logic
    // -------------------------------------------------------------------------
    [Authorize(Roles = "Customer,Admin")]
    public class CartController : Controller
    {
        private readonly ICartService _cart;

        // Constructor:
        //   Injects the ICartService to manage cart operations stored in session.
        public CartController(ICartService cart) => _cart = cart;

        // =====================================================================
        // GET: /Cart
        // Purpose:
        //   Displays the current shopping cart view for the logged-in user.
        //   Uses the service to retrieve items stored in session memory.
        // =====================================================================
        [HttpGet]
        public IActionResult Index()
        {
            var vm = _cart.GetCart(); // Retrieve cart from session
            return View(vm);          // Return view model to the Razor page
        }

        // =====================================================================
        // GET: /Cart/Add
        // Purpose:
        //   Friendly fallback to prevent HTTP 405 errors if user manually visits
        //   /Cart/Add in browser. Redirects back to product listing.
        // =====================================================================
        [HttpGet]
        public IActionResult Add()
        {
            TempData["CartMessage"] = "Use the product page to add items to your cart.";
            return RedirectToAction("Index", "Products");
        }

        // =====================================================================
        // POST: /Cart/Add
        // Purpose:
        //   Adds a selected product configuration (size, toppings, sugar, ice)
        //   to the current user's session cart. Validates form data and provides
        //   user feedback via TempData message.
        // Workflow:
        //   1. Validate the incoming CartItem model.
        //   2. Guard against null or invalid quantity.
        //   3. Add item to cart via ICartService.
        //   4. Redirect back to Cart Index with success message.
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add([FromForm] CartItem item)
        {
            if (item is null)
            {
                TempData["CartMessage"] = "Invalid item.";
                return RedirectToAction(nameof(Index));
            }

            // Ensure quantity is at least one
            item.Quantity = Math.Max(1, item.Quantity);

            // (Future enhancement) Server-side price verification for security
            _cart.Add(item);

            TempData["CartMessage"] = "Item added to cart.";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // POST: /Cart/UpdateQty
        // Purpose:
        //   Updates the quantity of a specific cart item (matching by product ID
        //   and selected customisation options such as size, sugar, ice, toppings).
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQty(
            string productId, string size, int sugar, int ice, string toppingsSummary, int qty)
        {
            // Enforce minimum quantity
            qty = Math.Max(1, qty);

            _cart.UpdateQty(productId, size, sugar, ice, toppingsSummary, qty);
            TempData["CartMessage"] = "Quantity updated.";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // POST: /Cart/Remove
        // Purpose:
        //   Removes a specific customised drink item from the session cart.
        //   Uses product ID and options to identify the correct item.
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(
            string productId, string size, int sugar, int ice, string toppingsSummary)
        {
            _cart.Remove(productId, size, sugar, ice, toppingsSummary);
            TempData["CartMessage"] = "Item removed.";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // POST: /Cart/Clear
        // Purpose:
        //   Clears all items from the session cart for the current user.
        //   Used during checkout completion or manual reset.
        // =====================================================================
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
