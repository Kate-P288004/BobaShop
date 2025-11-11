using System.ComponentModel.DataAnnotations;

namespace BobaShop.Web.Models;

public class EditToppingVm
{
    [Required, StringLength(60)]
    public string Name { get; set; } = "";

    [Range(0, 100)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}
