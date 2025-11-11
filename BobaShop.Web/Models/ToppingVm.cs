namespace BobaShop.Web.Models;

public class ToppingVm
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}
