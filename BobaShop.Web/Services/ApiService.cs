using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using BobaShop.Web.Models;

namespace BobaShop.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        public ApiService(HttpClient http) => _http = http;

        public async Task<List<ProductViewModel>> GetProductsAsync()
            => await _http.GetFromJsonAsync<List<ProductViewModel>>("https://localhost:5001/api/products");
    }
}
