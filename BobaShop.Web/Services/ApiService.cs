using System.Net.Http;
using System.Net.Http.Json;
using BobaShop.Web.Models;

namespace BobaShop.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        public ApiService(HttpClient http) => _http = http;

        public async Task<List<ProductViewModel>> GetProductsAsync()
        {
            // If the API returns null (e.g., 204), fall back to an empty list to avoid CS8603
            var data = await _http.GetFromJsonAsync<List<ProductViewModel>>("api/products");
            return data ?? new List<ProductViewModel>();
        }
    }
}
