namespace BobaShop.Web.Models
{
    public class DrinkVm
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal BasePrice { get; set; } = 0m;
        public decimal Price { get; set; } = 0m;

        public decimal SmallUpcharge { get; set; } = 0m;
        public decimal MediumUpcharge { get; set; } = 0m;
        public decimal LargeUpcharge { get; set; } = 0m;

        public int DefaultSugar { get; set; } = 50;
        public int DefaultIce { get; set; } = 50;

        public bool IsActive { get; set; } = true;

        public string? ImageUrl { get; set; }
        public string? ImageAlt { get; set; }

        // NEW — for Admin views
        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }
        public DateTime? DeletedUtc { get; set; }
    }
}
