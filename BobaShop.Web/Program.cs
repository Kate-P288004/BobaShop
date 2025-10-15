// Program.cs — BobaShop.Web
using BobaShop.Web.Data;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Services
// ------------------------------------------------------------

// MVC + Razor Pages (Identity UI uses Razor Pages)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// EF Core (SQLite) for Identity
// "ConnectionStrings": { "DefaultConnection": "Data Source=app_identity.db" }
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity (with UI)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // ok for assessment
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Identity cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Typed HttpClient for your API calls
// "ApiBaseUrl": "https://localhost:5001/"
builder.Services.AddHttpClient<ApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);
});

// -------------------- Cart (Session) --------------------
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromHours(2);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();
// --------------------------------------------------------

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

app.UseSession(); // <-- session must be before endpoint mapping

// MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity UI endpoints (Login/Register/Logout)
app.MapRazorPages();

app.Run();
