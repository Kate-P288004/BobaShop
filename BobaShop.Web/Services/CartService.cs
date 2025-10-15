using BobaShop.Web.Helpers;
using BobaShop.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BobaShop.Web.Services
{
    public interface ICartService
    {
        CartViewModel GetCart();
        void Add(CartItem item);
        void UpdateQty(string productId, string size, int sugar, int ice, string toppingsSummary, int qty);
        void Remove(string productId, string size, int sugar, int ice, string toppingsSummary);
        void Clear();
        int GetCount();
    }

    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _ctx;
        public CartService(IHttpContextAccessor ctx) => _ctx = ctx;

        private ISession S => _ctx.HttpContext!.Session;

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

        public void Save(CartViewModel cart) => S.SetObject(CartKeys.SessionKey, cart);

        public void Add(CartItem item)
        {
            var cart = GetCart();
            // Merge by product/customization combo
            var existing = cart.Items.FirstOrDefault(i =>
                i.ProductId == item.ProductId &&
                i.Size == item.Size &&
                i.Sugar == item.Sugar &&
                i.Ice == item.Ice &&
                i.ToppingsSummary == item.ToppingsSummary);

            if (existing is null) cart.Items.Add(item);
            else existing.Quantity += item.Quantity;

            Save(cart);
        }

        public void UpdateQty(string productId, string size, int sugar, int ice, string toppingsSummary, int qty)
        {
            var cart = GetCart();
            var it = cart.Items.FirstOrDefault(i =>
                i.ProductId == productId && i.Size == size && i.Sugar == sugar && i.Ice == ice && i.ToppingsSummary == toppingsSummary);
            if (it != null)
            {
                it.Quantity = Math.Max(1, qty);
                Save(cart);
            }
        }

        public void Remove(string productId, string size, int sugar, int ice, string toppingsSummary)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(i =>
                i.ProductId == productId && i.Size == size && i.Sugar == sugar && i.Ice == ice && i.ToppingsSummary == toppingsSummary);
            Save(cart);
        }

        public void Clear()
        {
            Save(new CartViewModel());
        }

        public int GetCount() => GetCart().Items.Sum(i => i.Quantity);
    }
}
