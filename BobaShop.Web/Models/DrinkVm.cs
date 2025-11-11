namespace BobaShop.Web.Models;

public class DrinkVm
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    // NEW: size upcharges (match API model)
    public decimal SmallUpcharge { get; set; }
    public decimal MediumUpcharge { get; set; }
    public decimal LargeUpcharge { get; set; }

    // NEW: defaults (match API model)
    public int DefaultSugar { get; set; }
    public int DefaultIce { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}
