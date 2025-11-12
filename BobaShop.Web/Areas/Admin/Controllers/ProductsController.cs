// -----------------------------------------------------------------------------
// File: Areas/Admin/Controllers/ProductsController.cs
// Project: BobaShop.Web
// Area: Admin
// Security: Requires "RequireAdmin" policy (JWT attached via IApiAuthService)
// Purpose:
//   Server-side MVC controller for admin product management in the Web app.
//   Talks to the API's Drinks endpoints using an authenticated HttpClient.
//   Provides list, create, edit, and delete flows with TempData feedback.
// Notes:
//   - All API calls go through IApiAuthService to include the bearer token.
//   - ModelState is enriched with API validation messages when available.
//   - Views live under Areas/Admin/Views/Products/*
// -----------------------------------------------------------------------------

using System.Net;
using System.Net.Http.Json;
using BobaShop.Web.Models;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "RequireAdmin")]
public class ProductsController : Controller
{
    private readonly IApiAuthService _apiAuth;

    // Dependency injection:
    // IApiAuthService creates HttpClient instances with the current user's JWT.
    public ProductsController(IApiAuthService apiAuth) => _apiAuth = apiAuth;

    // -------------------------------------------------------------------------
    // GET: /Admin/Products
    // Renders list of drinks for admins.
    // Flow:
    //   1) Build an authenticated client.
    //   2) Call API: GET api/v1/Drinks?take=200
    //   3) Handle 401 with a friendly TempData message.
    //   4) Deserialize to List<DrinkVm> and sort by Name before rendering.
    // On failure:
    //   Returns an empty list with TempData["Error"] set.
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var api = await _apiAuth.CreateClientAsync(User);

        var res = await api.GetAsync("api/v1/Drinks?take=200");
        if (!res.IsSuccessStatusCode)
        {
            TempData["Error"] = res.StatusCode == HttpStatusCode.Unauthorized
                ? "API 401 Unauthorized. Your admin token didn’t attach or expired."
                : $"API error {(int)res.StatusCode}";
            return View(new List<DrinkVm>());
        }

        var items = await res.Content.ReadFromJsonAsync<List<DrinkVm>>() ?? new List<DrinkVm>();
        return View(items.OrderBy(x => x.Name).ToList());
    }

    // -------------------------------------------------------------------------
    // GET: /Admin/Products/Create
    // Shows the create form with sensible defaults.
    // Defaults:
    //   Sugar 50, Ice 50, IsActive true.
    // -------------------------------------------------------------------------
    [HttpGet]
    public IActionResult Create()
    {
        var vm = new ProductCreateVm
        {
            DefaultSugar = 50,
            DefaultIce = 50,
            IsActive = true
        };
        return View(vm); // Areas/Admin/Views/Products/Create.cshtml
    }

    // -------------------------------------------------------------------------
    // POST: /Admin/Products/Create
    // Creates a new drink via API POST api/v1/Drinks.
    // Behavior:
    //   - Validates ModelState.
    //   - Sends a JSON payload matching API DTO fields.
    //   - On API validation failure, surfaces errors into ModelState.
    // UX:
    //   - TempData["Ok"] on success, TempData["Error"] on failure.
    //   - Redirects to Index after success to prevent resubmits.
    // -------------------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var api = await _apiAuth.CreateClientAsync(User);

        var payload = new
        {
            name = vm.Name,
            description = vm.Description,
            basePrice = vm.BasePrice,
            smallUpcharge = vm.SmallUpcharge,
            mediumUpcharge = vm.MediumUpcharge,
            largeUpcharge = vm.LargeUpcharge,
            defaultSugar = vm.DefaultSugar,
            defaultIce = vm.DefaultIce,
            isActive = vm.IsActive,
            imageUrl = vm.ImageUrl,
            imageAlt = vm.ImageAlt
        };

        var resp = await api.PostAsJsonAsync("api/v1/Drinks", payload);
        if (!resp.IsSuccessStatusCode)
        {
            await AddApiErrorsToModelState(resp);
            TempData["Error"] = $"API error {(int)resp.StatusCode}";
            return View(vm);
        }

        TempData["Ok"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

    // -------------------------------------------------------------------------
    // GET: /Admin/Products/Edit/{id}
    // Loads a drink and maps it to EditDrinkVm for the form.
    // Flow:
    //   1) Guard against missing id.
    //   2) GET api/v1/Drinks/{id}.
    //   3) Map response to EditDrinkVm fields used by the view.
    // ViewBag:
    //   - ViewBag.DrinkId used by the form for route generation.
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var api = await _apiAuth.CreateClientAsync(User);

        var res = await api.GetAsync($"api/v1/Drinks/{id}");
        if (res.StatusCode == HttpStatusCode.NotFound) return NotFound();
        if (!res.IsSuccessStatusCode)
        {
            TempData["Error"] = res.StatusCode == HttpStatusCode.Unauthorized
                ? "API 401 Unauthorized while loading product."
                : $"API error {(int)res.StatusCode}";
            return RedirectToAction(nameof(Index));
        }

        var drink = await res.Content.ReadFromJsonAsync<DrinkVm>();
        if (drink is null) return NotFound();

        var vm = new EditDrinkVm
        {
            Name = drink.Name ?? string.Empty,
            Description = drink.Description,
            BasePrice = drink.BasePrice,
            SmallUpcharge = drink.SmallUpcharge,
            MediumUpcharge = drink.MediumUpcharge,
            LargeUpcharge = drink.LargeUpcharge,
            DefaultSugar = drink.DefaultSugar,
            DefaultIce = drink.DefaultIce,
            IsActive = drink.IsActive
            // To support image editing, extend EditDrinkVm and map here.
        };

        ViewBag.DrinkId = drink.Id;
        return View(vm);
    }

    // -------------------------------------------------------------------------
    // POST: /Admin/Products/Edit/{id}
    // Updates a drink via API PUT api/v1/Drinks/{id}.
    // Behavior:
    //   - Validates ModelState and keeps DrinkId in ViewBag when redisplaying.
    //   - Sends only fields that API accepts for update.
    // UX:
    //   - TempData["Ok"] on success.
    //   - On error, pushes API details into ModelState and redisplays form.
    // -------------------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditDrinkVm vm)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.DrinkId = id;
            return View(vm);
        }

        var api = await _apiAuth.CreateClientAsync(User);

        var payload = new
        {
            name = vm.Name,
            description = vm.Description,
            basePrice = vm.BasePrice,
            smallUpcharge = vm.SmallUpcharge,
            mediumUpcharge = vm.MediumUpcharge,
            largeUpcharge = vm.LargeUpcharge,
            defaultSugar = vm.DefaultSugar,
            defaultIce = vm.DefaultIce,
            isActive = vm.IsActive
            // Include image fields here if Edit supports them later.
        };

        var resp = await api.PutAsJsonAsync($"api/v1/Drinks/{id}", payload);
        if (!resp.IsSuccessStatusCode)
        {
            await AddApiErrorsToModelState(resp);
            TempData["Error"] = $"API error {(int)resp.StatusCode}";
            ViewBag.DrinkId = id;
            return View(vm);
        }

        TempData["Ok"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    // -------------------------------------------------------------------------
    // GET: /Admin/Products/Delete/{id}
    // Shows a confirmation page populated from the API.
    // Behavior:
    //   - If API returns 404, show NotFound to match route semantics.
    //   - For other errors, redirect back to Index with a message.
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var api = await _apiAuth.CreateClientAsync(User);

        var res = await api.GetAsync($"api/v1/Drinks/{id}");
        if (res.StatusCode == HttpStatusCode.NotFound) return NotFound();
        if (!res.IsSuccessStatusCode)
        {
            TempData["Error"] = res.StatusCode == HttpStatusCode.Unauthorized
                ? "API 401 Unauthorized while loading product."
                : $"API error {(int)res.StatusCode}";
            return RedirectToAction(nameof(Index));
        }

        var drink = await res.Content.ReadFromJsonAsync<DrinkVm>();
        if (drink is null) return NotFound();

        return View(drink);
    }

    // -------------------------------------------------------------------------
    // POST: /Admin/Products/Delete/{id}
    // Confirms deletion via API DELETE api/v1/Drinks/{id}.
    // Behavior:
    //   - On 401 or other failures, show a message and bounce back to Delete page.
    //   - On success, redirect to Index with a success banner.
    // -------------------------------------------------------------------------
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var api = await _apiAuth.CreateClientAsync(User);

        var resp = await api.DeleteAsync($"api/v1/Drinks/{id}");
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = resp.StatusCode == HttpStatusCode.Unauthorized
                ? "API 401 Unauthorized while deleting product."
                : $"API error {(int)resp.StatusCode}";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Ok"] = "Product deleted.";
        return RedirectToAction(nameof(Index));
    }

    // -------------------------------------------------------------------------
    // Helper: AddApiErrorsToModelState
    // Purpose:
    //   Reads error details from the API response and adds them to ModelState.
    // Supports:
    //   - ValidationProblemDetails with field-level errors.
    //   - ProblemDetails with a single detail message.
    //   - Raw response body as a last resort.
    // Rationale:
    //   Keeps the user on the form with actionable messages.
    // -------------------------------------------------------------------------
    private async Task AddApiErrorsToModelState(HttpResponseMessage resp)
    {
        // Try ValidationProblemDetails shape first (field-level errors)
        try
        {
            var vpd = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            if (vpd?.Errors?.Count > 0)
            {
                foreach (var kv in vpd.Errors)
                {
                    var key = string.IsNullOrWhiteSpace(kv.Key) ? string.Empty : kv.Key;
                    foreach (var msg in kv.Value)
                        ModelState.AddModelError(key, msg);
                }
                return;
            }
        }
        catch { /* ignore parse errors */ }

        // Fallback: ProblemDetails with a general message
        try
        {
            var pd = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
            if (!string.IsNullOrWhiteSpace(pd?.Detail))
            {
                ModelState.AddModelError(string.Empty, pd.Detail);
                return;
            }
        }
        catch { /* ignore parse errors */ }

        // Last resort: dump raw body to ModelState
        var body = await resp.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"API error {(int)resp.StatusCode}: {body}");
    }
}
