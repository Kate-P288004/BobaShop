using BobaShop.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.ViewComponents
{
   
    [ViewComponent(Name = "CartBadge")]
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ICartService _cart;
        public CartBadgeViewComponent(ICartService cart) => _cart = cart;

        public IViewComponentResult Invoke()
        {
            var count = _cart.GetCount();
            return View(count); // looks for Views/Shared/Components/CartBadge/Default.cshtml
        }
    }
}
