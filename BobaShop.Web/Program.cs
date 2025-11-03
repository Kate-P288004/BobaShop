// ---------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Web (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: October 2025
// Purpose:
//   - Configures ASP.NET Core MVC with Identity (SQLite)
//   - Registers HttpClient for API v1 integration
//   - Adds Session, Cart, and Rewards services
//   - Seeds default Admin + roles
// ---------------------------------------------------------------

using BobaShop.Web.Data;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 1) DATABASE (SQLite for Identity)
// -----------------------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("IdentityDb")));

// -----------------------------------------------------------------------------
// 2) IDENTITY (ApplicationUser + Roles)
// -----------------------------------------------------------------------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(opts =>
    {
        opts.Password.RequireDigit = true;
        opts.Password.RequireLowercase = true;
        opts.Password.RequireUppercase = false;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequiredLength = 8;
        opts.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.LogoutPath = "/Account/Logout";
    o.AccessDeniedPath = "/Account/AccessDenied";
});

// -----------------------------------------------------------------------------
// 3) MVC + Razor Views
// -----------------------------------------------------------------------------
builder.Services.AddControllersWithViews();

// -----------------------------------------------------------------------------
// 4) SESSION + CART SERVICE
// -----------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddScoped<ICartService, CartService>();

// -----------------------------------------------------------------------------
// 5) REWARDS SERVICE
// -----------------------------------------------------------------------------
builder.Services.AddScoped<IRewardsService, RewardsService>();

// -----------------------------------------------------------------------------
// 6) API CLIENT (HttpClient for v1 endpoints)
// -----------------------------------------------------------------------------
builder.Services.AddHttpClient<IDrinksApi, DrinksApi>(client =>
{
    // Base URL is stored in appsettings.json: "Api:BaseUrl": "https://localhost:5001/api/v1/"
    var apiBase = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrEmpty(apiBase))
        throw new InvalidOperationException("Missing API base URL in appsettings.json.");

    client.BaseAddress = new Uri(apiBase);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// -----------------------------------------------------------------------------
// 7) IDENTITY SEEDER (Roles + Admin user)
// -----------------------------------------------------------------------------
builder.Services.AddScoped<IdentitySeeder>();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 8) APPLY MIGRATIONS + SEED ROLES/ADMIN
// -----------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await seeder.SeedAsync(
        app.Configuration["AdminSeed:Email"] ?? "admin@bobatastic.local",
        app.Configuration["AdminSeed:Password"] ?? "Admin!23456",
        app.Configuration["AdminSeed:Role"] ?? "Admin"
    );
}

// -----------------------------------------------------------------------------
// 9) MIDDLEWARE PIPELINE
// -----------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session must be before MVC endpoints
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// -----------------------------------------------------------------------------
// Identity Seeder (creates roles + admin on startup)
// -----------------------------------------------------------------------------
public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole> _roles;
    private readonly UserManager<ApplicationUser> _users;

    public IdentitySeeder(RoleManager<IdentityRole> roles, UserManager<ApplicationUser> users)
    {
        _roles = roles;
        _users = users;
    }

    public async Task SeedAsync(string adminEmail, string adminPassword, string adminRole)
    {
        var roles = new[] { "Admin", "Customer" };
        foreach (var r in roles)
        {
            if (!await _roles.RoleExistsAsync(r))
                await _roles.CreateAsync(new IdentityRole(r));
        }

        var admin = await _users.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Site Administrator",
                RewardPoints = 0
            };

            var result = await _users.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await _users.AddToRoleAsync(admin, adminRole);
            }
        }
    }
}
