// ---------------------------------------------------------------
// File: DrinksApi.cs
// Project: BobaShop.Web (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: October 2025
// Purpose:
//   Service to call the BoBatastic API for Drinks data.
//   This keeps Web MVC separate from API logic and uses HttpClient.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BobaShop.Web.Services
{
    // -------------------------
    // ViewModel for Drink items
    // -------------------------
    public sealed class DrinkVm
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // -------------------------
    // Interface for Drinks API
    // -------------------------
    public interface IDrinksApi
    {
        Task<IReadOnlyList<DrinkVm>> GetAllAsync(string? name = null);
        Task<DrinkVm?> GetByIdAsync(string id);
    }

    // -------------------------
    // Implementation
    // -------------------------
    public sealed class DrinksApi : IDrinksApi
    {
        private readonly HttpClient _http;

        public DrinksApi(HttpClient http)
        {
            _http = http;
        }

        // -----------------------------------------------------------
        // GET: /api/v1/drinks
        // -----------------------------------------------------------
        public async Task<IReadOnlyList<DrinkVm>> GetAllAsync(string? name = null)
        {
            var url = "drinks"; // base URL is set in appsettings.json: /api/v1/
            if (!string.IsNullOrWhiteSpace(name))
                url += $"?name={Uri.EscapeDataString(name)}";

            var result = await _http.GetFromJsonAsync<List<DrinkVm>>(url);
            return result ?? new List<DrinkVm>();
        }

        // -----------------------------------------------------------
        // GET: /api/v1/drinks/{id}
        // -----------------------------------------------------------
        public async Task<DrinkVm?> GetByIdAsync(string id)
        {
            try
            {
                return await _http.GetFromJsonAsync<DrinkVm>($"drinks/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
