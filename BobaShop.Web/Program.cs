// Program.cs — BobaShop.Web

using System;
using BobaShop.Web.Data;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Services
// ------------------------------------------------------------

// MVC + Razor Pages (Identity UI uses Razor Pages under the hood)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// EF Core (SQLite) for Identity
// appsettings.json must contain:
// "ConnectionStrings": { "DefaultConnection": "Data Source=app_identity.db" }
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity (with UI)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // fine for assessment
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Identity cookie paths (redirect unauthenticated users to Login)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Typed HttpClient for your API calls
// appsettings.json must contain:
// "ApiBaseUrl": "https://localhost:5001/"
builder.Services.AddHttpClient<ApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// ------------------------------------------------------------
// Middleware pipeline
// ------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity UI endpoints (Login/Register/Logout)
app.MapRazorPages();

app.Run();
