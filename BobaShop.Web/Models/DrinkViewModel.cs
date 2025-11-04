namespace BobaShop.Web.Models
{
    public class DrinkViewModel
    {
        public string Id { get; set; } = string.Empty;     // use a slug or GUID
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }                 // money to decimal
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
    }
}
