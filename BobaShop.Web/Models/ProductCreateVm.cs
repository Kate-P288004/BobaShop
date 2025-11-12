namespace BobaShop.Web.Models
{
    public class ProductCreateVm
    {
        public string Name { get; set; } = "";
        public decimal BasePrice { get; set; }
        public decimal SmallUpcharge { get; set; }
        public decimal MediumUpcharge { get; set; }
        public decimal LargeUpcharge { get; set; }
        public int DefaultSugar { get; set; } = 50;
        public int DefaultIce { get; set; } = 50;
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImageAlt { get; set; }
    }
}
