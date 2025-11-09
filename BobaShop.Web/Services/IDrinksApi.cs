// -----------------------------------------------------------------------------
// File: Services/IDrinksApi.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Contract for the MVC frontend to communicate with the BobaShop.Api layer.
//   Implementations use HttpClient to call the REST endpoints and return
//   simple view models for Razor pages.
// -----------------------------------------------------------------------------

using BobaShop.Web.Models;

namespace BobaShop.Web.Services
{
    /// <summary>
    /// Abstraction over the Drinks API endpoints used by the MVC site.
    /// Keep method signatures simple and UI-focused.
    /// </summary>
    public interface IDrinksApi
    {
        /// <summary>
        /// Retrieves all drinks from the API (anonymous GET).
        /// Returns an empty list if the API is unreachable.
        /// </summary>
        /// <returns>List of drinks for the catalogue/menu page.</returns>
        Task<List<DrinkVm>> GetAllAsync();

        /// <summary>
        /// Retrieves one drink by its ObjectId string from the API (anonymous GET).
        /// </summary>
        /// <param name="id">MongoDB ObjectId (24-char hex string).</param>
        /// <returns>The drink if found; otherwise null.</returns>
        Task<DrinkVm?> GetByIdAsync(string id);
    }
}
