// -----------------------------------------------------------------------------
// File: Models/DrinkVm.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Represents a simplified version of the Drink object returned by the API.
// -----------------------------------------------------------------------------

namespace BobaShop.Web.Models
{
    public class DrinkVm
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public decimal BasePrice { get; set; }
    }
}
