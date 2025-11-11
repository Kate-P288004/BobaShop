using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace BobaShop.Web.Services;

public interface IApiAuthService
{
    Task<HttpClient> CreateClientAsync(ClaimsPrincipal user);
}

public sealed class ApiAuthService : IApiAuthService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _cfg;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "Api:BearerToken";

    public ApiAuthService(IHttpClientFactory httpFactory, IConfiguration cfg, IMemoryCache cache)
    {
        _httpFactory = httpFactory;
        _cfg = cfg;
        _cache = cache;
    }

    public async Task<HttpClient> CreateClientAsync(ClaimsPrincipal user)
    {
        var client = _httpFactory.CreateClient();

        var baseUrl = _cfg["Api:BaseUrl"] ?? "https://localhost:7274/";
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var token = await GetOrFetchTokenAsync(client);
        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<string> GetOrFetchTokenAsync(HttpClient client)
    {
        if (_cache.TryGetValue<string>(CacheKey, out var cached))
            return cached!;

        var email = _cfg["Api:ServiceUser:Email"] ?? "admin@bobatastic.local";
        var password = _cfg["Api:ServiceUser:Password"] ?? "Admin!23456";

        var payload = JsonSerializer.Serialize(new { email, password });
        using var resp = await client.PostAsync("api/v1/Auth/login",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!resp.IsSuccessStatusCode) return string.Empty;

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // match API response keys
        var token = doc.RootElement.TryGetProperty("access_token", out var t) ? t.GetString() : null;
        var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var e) ? e.GetInt32() : 3600;

        if (!string.IsNullOrWhiteSpace(token))
        {
            // pad a little to avoid edge expiry
            _cache.Set(CacheKey, token, TimeSpan.FromSeconds(Math.Max(60, expiresIn - 120)));
            return token!;
        }

        return string.Empty;
    }
}
