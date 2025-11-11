using System.ComponentModel.DataAnnotations;

namespace BobaShop.Web.Models;

public class EditDrinkVm
{
    [Required, StringLength(60)]
    public string Name { get; set; } = "";

    [Range(0, 1000)]
    public decimal BasePrice { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    [Range(0, 10)]
    public decimal SmallUpcharge { get; set; }

    [Range(0, 10)]
    public decimal MediumUpcharge { get; set; }

    [Range(0, 10)]
    public decimal LargeUpcharge { get; set; }

    [Range(0, 100)]
    public int DefaultSugar { get; set; } = 50;

    [Range(0, 100)]
    public int DefaultIce { get; set; } = 50;

    public bool IsActive { get; set; } = true;
}
