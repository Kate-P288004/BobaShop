using System.ComponentModel.DataAnnotations;

namespace BobaShop.Api.Dtos
{
    public class DrinkCreateDto
    {
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(400)]
        public string? Description { get; set; }

        [Range(0, 9999)] // zero allowed
        public decimal BasePrice { get; set; }

        [Range(0, 9999)]
        public decimal SmallUpcharge { get; set; }

        [Range(0, 9999)]
        public decimal MediumUpcharge { get; set; }

        [Range(0, 9999)]
        public decimal LargeUpcharge { get; set; }

        [Range(0, 100)]
        public int DefaultSugar { get; set; } = 50;

        [Range(0, 100)]
        public int DefaultIce { get; set; } = 50;

        public bool IsActive { get; set; } = true;

        // images
        public string? ImageUrl { get; set; }
        public string? ImageAlt { get; set; }
    }
}
