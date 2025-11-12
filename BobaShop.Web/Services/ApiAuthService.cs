// -----------------------------------------------------------------------------
// File: Services/ApiAuthService.cs
// Project: BobaShop.Web (BoBatastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Provides a centralized authentication service for secure API communication
//   between the BoBatastic Web frontend (MVC) and the BobaShop.Api backend.
//
//   The service handles JWT-based authorization by automatically logging in
//   using a configured service account (defined in appsettings.json) and caching
//   the bearer token in memory to minimize authentication requests.
//
//   Key Concepts Demonstrated:
//     • Dependency Injection with IHttpClientFactory and IMemoryCache
//     • JWT authentication and header injection
//     • Secure API token caching and automatic renewal
//     • Separation of concerns: AuthService abstracts API auth logic
// -----------------------------------------------------------------------------

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace BobaShop.Web.Services
{
    // -------------------------------------------------------------------------
    // Interface: IApiAuthService
    // Purpose:
    //   Defines a service contract for creating pre-authenticated HttpClient
    //   instances used for secure API communication.
    //
    // Implementation:
    //   - ApiAuthService provides the default implementation.
    // -------------------------------------------------------------------------
    public interface IApiAuthService
    {
        // Creates and configures an HttpClient with an attached JWT Bearer token.
        Task<HttpClient> CreateClientAsync(ClaimsPrincipal user);
    }

    // -------------------------------------------------------------------------
    // Class: ApiAuthService
    // Purpose:
    //   Handles authentication with the BoBaTastic API by obtaining and caching
    //   a valid JWT token using the configured service account credentials.
    //   Ensures each HttpClient used by Admin controllers carries valid headers.
    // -------------------------------------------------------------------------
    public sealed class ApiAuthService : IApiAuthService
    {
        private readonly IHttpClientFactory _httpFactory;  // For creating isolated clients
        private readonly IConfiguration _cfg;              // App configuration (appsettings.json)
        private readonly IMemoryCache _cache;              // In-memory cache for tokens
        private const string CacheKey = "Api:BearerToken"; // Cache key for stored token

        // ---------------------------------------------------------------------
        // Constructor:
        //   Injects IHttpClientFactory, IConfiguration, and IMemoryCache.
        //   Enables dependency injection from Startup.cs / Program.cs.
        // ---------------------------------------------------------------------
        public ApiAuthService(IHttpClientFactory httpFactory, IConfiguration cfg, IMemoryCache cache)
        {
            _httpFactory = httpFactory;
            _cfg = cfg;
            _cache = cache;
        }

        // ---------------------------------------------------------------------
        // Method: CreateClientAsync
        // Purpose:
        //   Creates an HttpClient configured with:
        //     • BaseAddress (from appsettings: Api:BaseUrl)
        //     • Accept header for JSON
        //     • Bearer token for authentication
        //
        // Usage:
        //   Called by Admin controllers (e.g., ProductsController, ToppingsController)
        //   before sending API requests.
        // ---------------------------------------------------------------------
        public async Task<HttpClient> CreateClientAsync(ClaimsPrincipal user)
        {
            var client = _httpFactory.CreateClient();

            // Load and normalize API base URL
            var baseUrl = _cfg["Api:BaseUrl"] ?? "https://localhost:7274/";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);

            // Ensure JSON format is accepted
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Retrieve or fetch a valid JWT token
            var token = await GetOrFetchTokenAsync(client);
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // ---------------------------------------------------------------------
        // Method: GetOrFetchTokenAsync
        // Purpose:
        //   Retrieves a cached JWT token if valid, or logs into the API to obtain
        //   a new one using service credentials. Token is cached with a reduced
        //   expiry time to prevent edge-case timeouts.
        //
        // Process:
        //   1. Check IMemoryCache for existing token.
        //   2. If not cached, send POST /api/v1/Auth/login to API.
        //   3. Extract "access_token" and "expires_in" fields from response.
        //   4. Store token in cache for reuse.
        // ---------------------------------------------------------------------
        private async Task<string> GetOrFetchTokenAsync(HttpClient client)
        {
            // Step 1: Check cache
            if (_cache.TryGetValue<string>(CacheKey, out var cached))
                return cached!;

            // Step 2: Load service credentials from configuration
            var email = _cfg["Api:ServiceUser:Email"] ?? "admin@bobatastic.local";
            var password = _cfg["Api:ServiceUser:Password"] ?? "Admin!23456";

            // Step 3: Send login request to API
            var payload = JsonSerializer.Serialize(new { email, password });
            using var resp = await client.PostAsync(
                "api/v1/Auth/login",
                new StringContent(payload, Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
                return string.Empty;

            // Step 4: Parse token response
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var token = doc.RootElement.TryGetProperty("access_token", out var t)
                ? t.GetString()
                : null;

            var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var e)
                ? e.GetInt32()
                : 3600; // Default 1 hour fallback

            // Step 5: Cache the token safely with buffer time
            if (!string.IsNullOrWhiteSpace(token))
            {
                // Store token slightly shorter than expiry to prevent race conditions
                _cache.Set(CacheKey, token, TimeSpan.FromSeconds(Math.Max(60, expiresIn - 120)));
                return token!;
            }

            return string.Empty;
        }
    }
}
