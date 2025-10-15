namespace BobaShop.Web.Models
{
    public static class CartKeys
    {
        public const string SessionKey = "CART";
    }

    public class CartItem
    {
        public string ProductId { get; set; } = "";
        public string Name { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Size { get; set; } = "S"; // S/M/L
        public int Sugar { get; set; } = 100;   // 0..100
        public int Ice { get; set; } = 100;     // 0..100
        public string ToppingsSummary { get; set; } = ""; // "Pearls(+0.80), Pudding(+1.00)"
        public double UnitPrice { get; set; }   // includes size + toppings
        public int Quantity { get; set; } = 1;

        public double Subtotal => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public double Subtotal => Items.Sum(i => i.Subtotal);
        public double Tax => 0; // optional
        public double Total => Subtotal + Tax;
    }
}
