// -----------------------------------------------------------------------------
// File: Services/DrinksApiService.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Implements IDrinksApi using HttpClient to call the REST API endpoints.
// -----------------------------------------------------------------------------

using System.Net.Http.Json;
using BobaShop.Web.Models;

namespace BobaShop.Web.Services
{
    public class DrinksApiService : IDrinksApi
    {
        private readonly HttpClient _http;

        public DrinksApiService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Returns a list of all drinks from the API.
        /// </summary>
        public async Task<List<DrinkVm>> GetAllAsync()
        {
            var data = await _http.GetFromJsonAsync<List<DrinkVm>>("api/v1/drinks");
            return data ?? new List<DrinkVm>();
        }

        /// <summary>
        /// Returns a single drink by its ObjectId string.
        /// </summary>
        public async Task<DrinkVm?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<DrinkVm>($"api/v1/drinks/{id}");
        }
    }
}
