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

    public ProductsController(IApiAuthService apiAuth) => _apiAuth = apiAuth;

    // -------------------- LIST --------------------
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

    // -------------------- CREATE --------------------
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

    // -------------------- EDIT --------------------
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
            // If you later add image editing, extend EditDrinkVm and map here.
        };

        ViewBag.DrinkId = drink.Id;
        return View(vm);
    }

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

    // -------------------- DELETE --------------------
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

    // -------------------- Helpers --------------------
    private async Task AddApiErrorsToModelState(HttpResponseMessage resp)
    {
        // Try ValidationProblemDetails shape first
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

        // Fallback: plain ProblemDetails or raw text
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

        var body = await resp.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"API error {(int)resp.StatusCode}: {body}");
    }
}
