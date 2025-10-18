// BobaShop.Web/Models/ProductViewModels.cs
using System.Collections.Generic;

namespace BobaShop.Web.Models
{
    public class ProductViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }          // <-- decimal
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ToppingOption
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }          // <-- decimal
        public bool Selected { get; set; }
    }

    public class ProductDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }      // <-- decimal

        public string Size { get; set; } = "S";
        public int Sugar { get; set; } = 100;
        public int Ice { get; set; } = 100;

        public List<ToppingOption> Toppings { get; set; } = new();
        public decimal CalculatedPrice => BasePrice;
    }
}
