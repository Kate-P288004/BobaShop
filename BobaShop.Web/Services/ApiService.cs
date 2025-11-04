// -----------------------------------------------------------------------------
// File: Services/ApiService.cs
// Project: BobaShop.Web (BoBaTastic Frontend)
// Student: Kate Odabas (P288004)
// Description:
//   Provides a lightweight HTTP client service for communicating with the
//   BoBaTastic API. 
// -----------------------------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Json;
using BobaShop.Web.Models;

namespace BobaShop.Web.Services
{
    // -------------------------------------------------------------------------
    // Class: ApiService
    // Purpose:
    //   Provides asynchronous methods to call BoBaTastic API endpoints.
    //   Uses HttpClient for making GET requests to retrieve
    //   products and convert JSON responses into ViewModels.
    // -------------------------------------------------------------------------
    public class ApiService
    {
        private readonly HttpClient _http;

        // ---------------------------------------------------------------------
        // Constructor:
        //   HttpClient is injected by the dependency injection container.
        //   The base address (API root URL) is configured in Program.cs.
        // ---------------------------------------------------------------------
        public ApiService(HttpClient http) => _http = http;

        // ---------------------------------------------------------------------
        // Method: GetProductsAsync
        // Example Usage:
        //   var products = await _apiService.GetProductsAsync();
        // ---------------------------------------------------------------------
        public async Task<List<ProductViewModel>> GetProductsAsync()
        {
            // Make GET request and parse JSON result
            var data = await _http.GetFromJsonAsync<List<ProductViewModel>>("api/products");

            // Guard against null 
            return data ?? new List<ProductViewModel>();
        }
    }
}
