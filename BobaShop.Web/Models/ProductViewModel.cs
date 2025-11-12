using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BobaShop.Web.Models
{
    public class ProductViewModel
    {
        public string? Id { get; set; }

        // Used by Details view for topping options
        public List<ToppingVm> Toppings { get; set; } = new();

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Range(0, 1000)]
        public decimal BasePrice { get; set; } = 0m;

        [Range(0, 1000)]
        public decimal Price { get; set; } = 0m;

        [Range(0, 1000)]
        public decimal SmallUpcharge { get; set; } = 0m;

        [Range(0, 1000)]
        public decimal MediumUpcharge { get; set; } = 0m;

        [Range(0, 1000)]
        public decimal LargeUpcharge { get; set; } = 0m;

        [Range(0, 100)]
        public int DefaultSugar { get; set; } = 50;

        [Range(0, 100)]
        public int DefaultIce { get; set; } = 50;

        public bool IsActive { get; set; } = true;

        public string? ImageUrl { get; set; }
        public string? ImageAlt { get; set; }

        // Some Admin views touch audit fields; keep nullable for ?. usage
        public DateTime? CreatedUtc { get; set; }
        public DateTime? UpdatedUtc { get; set; }
        public DateTime? DeletedUtc { get; set; }
    }
}
