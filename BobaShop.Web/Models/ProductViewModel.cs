namespace BobaShop.Web.Models
{
    public class ProductViewModel
    {
        public string? Id { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}
