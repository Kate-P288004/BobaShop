using System.ComponentModel.DataAnnotations;

namespace BobaShop.Api.Dtos
{
    public class DrinkUpdateDto
    {
        // Not required; we’ll take it from the route
        public string? Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(400)]
        public string? Description { get; set; }

        [Range(0, 9999)] public decimal BasePrice { get; set; }
        [Range(0, 9999)] public decimal SmallUpcharge { get; set; }
        [Range(0, 9999)] public decimal MediumUpcharge { get; set; }
        [Range(0, 9999)] public decimal LargeUpcharge { get; set; }
        [Range(0, 100)] public int DefaultSugar { get; set; } = 50;
        [Range(0, 100)] public int DefaultIce { get; set; } = 50;
        public bool IsActive { get; set; } = true;

        public string? ImageUrl { get; set; }
        public string? ImageAlt { get; set; }
    }
}
