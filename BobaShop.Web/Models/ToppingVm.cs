namespace BobaShop.Web.Models
{
    public class ToppingVm
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Was IsAvailable in your earlier model; Admin views expect IsActive
        public bool IsActive { get; set; } = true;

        // NEW — for list/detail display
        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }
        public DateTime? DeletedUtc { get; set; }

        // Slug used by Details.cshtml for element ids
        public string Code =>
            (Name ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", "-");
    }
}
