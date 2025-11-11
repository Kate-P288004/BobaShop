using System.Text;
using System.Text.Json;
using BobaShop.Web.Models;
using BobaShop.Web.Services; // <-- add
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "RequireAdmin")]
public class ToppingsController : Controller
{
    private readonly IApiAuthService _apiAuth;

    public ToppingsController(IApiAuthService apiAuth)
    {
        _apiAuth = apiAuth;
    }

    // GET: Admin/Toppings
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var api = await _apiAuth.CreateClientAsync(User);
        var r = await api.GetAsync("api/v1/Toppings?take=200");
        if (!r.IsSuccessStatusCode)
        {
            TempData["Error"] = $"API error {(int)r.StatusCode}";
            return View(new List<ToppingVm>());
        }

        var items = JsonSerializer.Deserialize<List<ToppingVm>>(
            await r.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        return View(items.OrderBy(x => x.Name).ToList());
    }

    // --------------------- CREATE ---------------------

    [HttpGet]
    public IActionResult Create() => View(new EditToppingVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EditToppingVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var api = await _apiAuth.CreateClientAsync(User);
        var payload = JsonSerializer.Serialize(new
        {
            name = vm.Name,
            price = vm.Price,
            isActive = vm.IsActive
        });

        var resp = await api.PostAsync("api/v1/Toppings",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!resp.IsSuccessStatusCode)
        {
            var msg = $"API error {(int)resp.StatusCode}";
            ModelState.AddModelError(string.Empty, msg);
            TempData["Error"] = msg;
            return View(vm);
        }

        TempData["Ok"] = "Topping created.";
        return RedirectToAction(nameof(Index));
    }

    // ---------------------- EDIT ----------------------

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var api = await _apiAuth.CreateClientAsync(User);
        var r = await api.GetAsync($"api/v1/Toppings/{id}");
        if (r.StatusCode == System.Net.HttpStatusCode.NotFound) return NotFound();
        if (!r.IsSuccessStatusCode)
        {
            TempData["Error"] = $"API error {(int)r.StatusCode}";
            return RedirectToAction(nameof(Index));
        }

        var t = JsonSerializer.Deserialize<ToppingVm>(
            await r.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (t is null) return NotFound();

        var vm = new EditToppingVm
        {
            Name = t.Name,
            Price = t.Price,
            IsActive = t.IsActive
        };

        ViewBag.ToppingId = t.Id;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditToppingVm vm)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.ToppingId = id;
            return View(vm);
        }

        var api = await _apiAuth.CreateClientAsync(User);
        var payload = JsonSerializer.Serialize(new
        {
            name = vm.Name,
            price = vm.Price,
            isActive = vm.IsActive
        });

        var resp = await api.PutAsync(
            $"api/v1/Toppings/{id}",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!resp.IsSuccessStatusCode)
        {
            var msg = $"API error {(int)resp.StatusCode}";
            ModelState.AddModelError(string.Empty, msg);
            TempData["Error"] = msg;
            ViewBag.ToppingId = id;
            return View(vm);
        }

        TempData["Ok"] = "Topping updated.";
        return RedirectToAction(nameof(Index));
    }

    // --------------------- DELETE ---------------------

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var api = await _apiAuth.CreateClientAsync(User);
        var r = await api.GetAsync($"api/v1/Toppings/{id}");
        if (r.StatusCode == System.Net.HttpStatusCode.NotFound) return NotFound();
        if (!r.IsSuccessStatusCode)
        {
            TempData["Error"] = $"API error {(int)r.StatusCode}";
            return RedirectToAction(nameof(Index));
        }

        var t = JsonSerializer.Deserialize<ToppingVm>(
            await r.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (t is null) return NotFound();
        return View(t);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var api = await _apiAuth.CreateClientAsync(User);
        var resp = await api.DeleteAsync($"api/v1/Toppings/{id}");
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = $"API error {(int)resp.StatusCode}";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Ok"] = "Topping deleted.";
        return RedirectToAction(nameof(Index));
    }
}
