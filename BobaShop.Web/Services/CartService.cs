// -----------------------------------------------------------------------------
// File: Services/CartService.cs
// Project: BobaShop.Web 
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Implements the shopping cart business logic for the BoBaTastic web app.
//   The cart is stored in session using JSON serialization.
//   Provides add, update, remove, clear, and count functionality, ensuring
//   persistence between requests for signed-in users.
// ----------------------------------------------------------------------------

using BobaShop.Web.Helpers;
using BobaShop.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BobaShop.Web.Services
{
    // -------------------------------------------------------------------------
    // Interface: ICartService
    // Purpose:
    //   Defines the contract for shopping cart operations. Abstracting the cart
    //   logic through an interface allows easy testing, replacement, or future
    //   integration with persistent (NoSQL/Redis) storage.
    // -------------------------------------------------------------------------
    public interface ICartService
    {
        CartViewModel GetCart();
        void Add(CartItem item);
        void UpdateQty(string productId, string size, int sugar, int ice, string toppingsSummary, int qty);
        void Remove(string productId, string size, int sugar, int ice, string toppingsSummary);
        void Clear();
        int GetCount();
    }

    // -------------------------------------------------------------------------
    // Class: CartService
    // Purpose:
    //   Provides a session-backed implementation of ICartService.
    //   Uses JSON serialization to store and retrieve CartViewModel from
    //   the user’s session.
    // -------------------------------------------------------------------------
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _ctx;

        // Constructor: receives IHttpContextAccessor for accessing session state.
        public CartService(IHttpContextAccessor ctx) => _ctx = ctx;

        // Shortcut property for the active session
        private ISession S => _ctx.HttpContext!.Session;

        // =====================================================================
        // Method: GetCart
        // Purpose:
        //   Retrieves the current CartViewModel from session.
        //   If no cart exists yet, a new one is created and stored.
        // Returns:
        //   The active CartViewModel for this user session.
        // =====================================================================
        public CartViewModel GetCart()
        {
            var cart = S.GetObject<CartViewModel>(CartKeys.SessionKey);
            if (cart is null)
            {
                cart = new CartViewModel();
                S.SetObject(CartKeys.SessionKey, cart);
            }
            return cart;
        }

        // Saves the current cart state back to session
        private void Save(CartViewModel cart) => S.SetObject(CartKeys.SessionKey, cart);

        // =====================================================================
        // Method: Add
        // Purpose:
        //   Adds an item to the shopping cart. If an identical product
        //   configuration already exists (same size/sugar/ice/toppings),
        //   it increments its quantity instead of duplicating.
        // Parameters:
        //   item – CartItem object representing the chosen drink
        // =====================================================================
        public void Add(CartItem item)
        {
            var cart = GetCart();

            // Merge duplicates based on customisation attributes
            var existing = cart.Items.FirstOrDefault(i =>
                i.ProductId == item.ProductId &&
                i.Size == item.Size &&
                i.Sugar == item.Sugar &&
                i.Ice == item.Ice &&
                i.ToppingsSummary == item.ToppingsSummary);

            if (existing is null)
                cart.Items.Add(item);
            else
                existing.Quantity += item.Quantity;

            Save(cart);
        }

        // =====================================================================
        // Method: UpdateQty
        // Purpose:
        //   Updates the quantity for a specific customised product.
        //   Prevents invalid quantities by enforcing a minimum of 1.
        // =====================================================================
        public void UpdateQty(string productId, string size, int sugar, int ice, string toppingsSummary, int qty)
        {
            var cart = GetCart();
            var it = cart.Items.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.Size == size &&
                i.Sugar == sugar &&
                i.Ice == ice &&
                i.ToppingsSummary == toppingsSummary);

            if (it != null)
            {
                it.Quantity = Math.Max(1, qty);
                Save(cart);
            }
        }

        // =====================================================================
        // Method: Remove
        // Purpose:
        //   Removes an item from the cart based on its unique configuration
        //   (product ID + size + sugar + ice + toppings combination).
        // =====================================================================
        public void Remove(string productId, string size, int sugar, int ice, string toppingsSummary)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(i =>
                i.ProductId == productId &&
                i.Size == size &&
                i.Sugar == sugar &&
                i.Ice == ice &&
                i.ToppingsSummary == toppingsSummary);
            Save(cart);
        }

        // =====================================================================
        // Method: Clear
        // Purpose:
        //   Empties the cart entirely. Typically used on checkout completion
        //   or when the user manually clears their cart.
        // =====================================================================
        public void Clear()
        {
            Save(new CartViewModel());
        }

        // =====================================================================
        // Method: GetCount
        // Purpose:
        //   Returns the total number of items (sum of quantities) currently in
        //   the cart. Useful for displaying a cart badge in the UI header.
        // =====================================================================
        public int GetCount() => GetCart().Items.Sum(i => i.Quantity);
    }
}
