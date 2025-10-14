// ---------------------------------------------------------------
// File: ProductViewModels.cs
// Project: BoBaTastic – Web UI
// ---------------------------------------------------------------
namespace BobaShop.Web.Models
{
    public class ProductViewModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public double Price { get; set; }           // base (Small)
        public string ImageUrl { get; set; } = "";
    }

    public class ToppingOption
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public bool Selected { get; set; }
    }

    public class ProductDetailsViewModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public double BasePrice { get; set; }       // Small size
        public string Size { get; set; } = "S";     // S/M/L
        public int Sugar { get; set; } = 100;       // 0..100
        public int Ice { get; set; } = 100;         // 0..100
        public List<ToppingOption> Toppings { get; set; } = new();
        public double CalculatedPrice => BasePrice; // (client JS updates UI)
    }
}
