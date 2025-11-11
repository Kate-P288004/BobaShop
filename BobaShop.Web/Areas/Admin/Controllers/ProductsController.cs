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

    // GET: /Admin/Products
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

    [HttpGet]
    public IActionResult Create() => View(new EditDrinkVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EditDrinkVm vm)
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
            isActive = vm.IsActive
        };

        var resp = await api.PostAsJsonAsync("api/v1/Drinks", payload);

        if (!resp.IsSuccessStatusCode)
        {
            var msg = resp.StatusCode == HttpStatusCode.Unauthorized
                ? "API 401 Unauthorized while creating product."
                : $"API error {(int)resp.StatusCode}";
            ModelState.AddModelError(string.Empty, msg);
            TempData["Error"] = msg;
            return View(vm);
        }

        TempData["Ok"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

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
        };

        var resp = await api.PutAsJsonAsync($"api/v1/Drinks/{id}", payload);

        if (!resp.IsSuccessStatusCode)
        {
            var msg = resp.StatusCode == HttpStatusCode.Unauthorized
                ? "API 401 Unauthorized while updating product."
                : $"API error {(int)resp.StatusCode}";
            ModelState.AddModelError(string.Empty, msg);
            TempData["Error"] = msg;
            ViewBag.DrinkId = id;
            return View(vm);
        }

        TempData["Ok"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

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
}
