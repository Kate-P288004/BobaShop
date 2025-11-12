// -----------------------------------------------------------------------------
// File: Areas/Admin/Controllers/ToppingsController.cs
// Project: BobaShop.Web (Admin area)
// Security: Requires "RequireAdmin" policy; API calls carry JWT via IApiAuthService
// Purpose:
//   Admin MVC controller for managing Toppings through the API.
//   Lists, creates, edits, and soft-deletes toppings by calling API v1 routes.
// Key ideas:
//   - Uses IApiAuthService to build HttpClient instances with the current user's token
//   - Surfaces API failures to the UI via TempData and ModelState
//   - Keeps view models small and focused; mapping happens at the edges
// Views:
//   Areas/Admin/Views/Toppings/Index.cshtml
//   Areas/Admin/Views/Toppings/Create.cshtml
//   Areas/Admin/Views/Toppings/Edit.cshtml
//   Areas/Admin/Views/Toppings/Delete.cshtml
// -----------------------------------------------------------------------------

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

    // DI: resolve IApiAuthService to obtain an authenticated HttpClient per request
    public ToppingsController(IApiAuthService apiAuth)
    {
        _apiAuth = apiAuth;
    }

    // -------------------------------------------------------------------------
    // GET: Admin/Toppings
    // Renders a paged list of toppings for administrators.
    // Flow:
    //   1) Build an HttpClient that includes the user's bearer token.
    //   2) Call API GET /api/v1/Toppings with a high take cap for admin browsing.
    //   3) If the API returns an error, show a friendly banner and an empty list.
    //   4) Otherwise, deserialize and sort by Name for predictable UI ordering.
    // Notes:
    //   - Uses System.Text.Json with case-insensitive mapping to be lenient.
    //   - Consider server-side paging for very large datasets later.
    // -------------------------------------------------------------------------
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

    // GET: Admin/Toppings/Create
    // Shows a blank form. Defaults come from EditToppingVm.
    [HttpGet]
    public IActionResult Create() => View(new EditToppingVm());

    // POST: Admin/Toppings/Create
    // Creates a topping through API POST /api/v1/Toppings.
    // Behavior:
    //   - Validates ModelState before calling the API.
    //   - Serializes only fields the API expects to keep payloads clean.
    // UX:
    //   - On success, redirect to Index with a success toast.
    //   - On error, keep the user on the form and show error details.
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

    // GET: Admin/Toppings/Edit/{id}
    // Loads a topping and maps it into EditToppingVm for the form.
    // Error handling:
    //   - 404 from API maps to MVC NotFound to keep semantics aligned.
    //   - Other failures show a banner and return to Index.
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

        // Used by the view to form correct action URLs
        ViewBag.ToppingId = t.Id;
        return View(vm);
    }

    // POST: Admin/Toppings/Edit/{id}
    // Updates a topping through API PUT /api/v1/Toppings/{id}.
    // Behavior:
    //   - Validates ModelState and preserves ViewBag.ToppingId when redisplaying.
    //   - Sends only the fields that are editable in the form.
    // UX:
    //   - Success -> redirect to Index with a toast.
    //   - Failure -> add error to ModelState and keep the user on the form.
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

    // GET: Admin/Toppings/Delete/{id}
    // Shows a confirmation screen populated with topping details.
    // Error handling matches Edit for consistency.
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

    // POST: Admin/Toppings/Delete/{id}
    // Confirms deletion. Calls API DELETE /api/v1/Toppings/{id}.
    // Behavior:
    //   - On error, redirect back to the Delete page so the user can retry.
    //   - On success, redirect to Index with a success toast.
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
