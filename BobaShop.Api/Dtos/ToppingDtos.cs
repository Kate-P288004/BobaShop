using System.ComponentModel.DataAnnotations;

namespace BobaShop.Api.Dtos
{
    public class ToppingCreateDto
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = default!;

        [Range(0, 1000)]
        public decimal Price { get; set; } = 0.80m;

        public bool IsActive { get; set; } = true;
    }

    public class ToppingUpdateDto : ToppingCreateDto { }
}
