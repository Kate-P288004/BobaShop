// -----------------------------------------------------------------------------
// File: ViewComponents/CartBadgeViewComponent.cs
// Project: BobaShop.Web (BoBatastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Provides a reusable ViewComponent that displays a small shopping-cart badge
//   showing the number of items currently in the user’s session cart.
//   The component retrieves cart data from ICartService and renders the count
//   dynamically in shared layouts (e.g., navbar, header, etc.).
//
//   Key Concepts Demonstrated:
//     • ASP.NET Core ViewComponent pattern for modular UI rendering
//     • Dependency Injection of a custom service (ICartService)
//     • Separation of presentation logic for partial views
//     • Integration with session-based cart storage
// -----------------------------------------------------------------------------

using BobaShop.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.ViewComponents
{
    // -------------------------------------------------------------------------
    // ViewComponent: CartBadge
    // Purpose:
    //   Displays a small cart icon or badge with the total number of items in
    //   the user's session cart. This component can be invoked directly from
    //   Razor layouts or partials using:
    //
    //       @await Component.InvokeAsync("CartBadge")
    //
    // Location:
    //   Views/Shared/Components/CartBadge/Default.cshtml
    // -------------------------------------------------------------------------
    [ViewComponent(Name = "CartBadge")]
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ICartService _cart;

        // ---------------------------------------------------------------------
        // Constructor:
        //   Injects the custom CartService (ICartService) which provides access
        //   to the session cart. Dependency injection is configured in Program.cs.
        // ---------------------------------------------------------------------
        public CartBadgeViewComponent(ICartService cart) => _cart = cart;

        // ---------------------------------------------------------------------
        // Method: Invoke
        // Purpose:
        //   Retrieves the total number of items in the cart and passes it to
        //   the corresponding Razor view for display.
        //
        // Returns:
        //   IViewComponentResult → Renders the cart count in the header.
        // ---------------------------------------------------------------------
        public IViewComponentResult Invoke()
        {
            var count = _cart.GetCount();
            return View(count); // Looks for Views/Shared/Components/CartBadge/Default.cshtml
        }
    }
}
