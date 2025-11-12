// ---------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Web (BoBaTastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// ---------------------------------------------------------------

using BobaShop.Web.Data;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 1) DATABASE (SQLite for Identity) — absolute path to identity.db
// -----------------------------------------------------------------------------
static string BuildIdentityConnection(WebApplicationBuilder b)
{
    var raw = b.Configuration.GetConnectionString("IdentityDb") ?? "Data Source=identity.db";
    const string prefix = "Data Source=";

    if (raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
    {
        var file = raw[prefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(file)) file = "identity.db";
        if (!Path.IsPathRooted(file))
            file = Path.Combine(b.Environment.ContentRootPath, file);

        return $"{prefix}{file}";
    }
    return raw;
}

var identityCs = BuildIdentityConnection(builder);

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlite(identityCs));

// Persist data protection keys (prevents cookie/sign-in issues across restarts)
var keysPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "BobaShop", "keys");
Directory.CreateDirectory(keysPath);

builder.Services
    .AddDataProtection()
    .SetApplicationName("BobaShop.Web")
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

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
    o.SlidingExpiration = true;
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// -----------------------------------------------------------------------------
// 3) MVC + Razor Views
// -----------------------------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(o => o.LowercaseUrls = true);

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
// 6) API AUTH + HTTP CLIENTS
// -----------------------------------------------------------------------------
builder.Services.AddMemoryCache();

// Generic client factory used by ApiAuthService
builder.Services.AddHttpClient();

// Auth service that fetches/refreshes JWT and adds it to outgoing requests
builder.Services.AddScoped<IApiAuthService, ApiAuthService>();

// Typed client for Drinks API calls
builder.Services.AddHttpClient<IDrinksApi, DrinksApiService>(client =>
{
    var apiBase = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7274/";
    if (!apiBase.EndsWith("/")) apiBase += "/";
    client.BaseAddress = new Uri(apiBase);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

// -----------------------------------------------------------------------------
// 7) IDENTITY SEEDER (Roles + Admin user)
// -----------------------------------------------------------------------------
builder.Services.AddScoped<IdentitySeeder>();

// -----------------------------------------------------------------------------
// 7.5) AUTHORIZATION POLICIES
// -----------------------------------------------------------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Log important paths and URLs
app.Logger.LogInformation("Identity DB: {cs}", identityCs);

// Resolve final API base from config/env to avoid confusion
var resolvedApiBase = app.Configuration["Api:BaseUrl"] ?? "https://localhost:7274/";
if (!resolvedApiBase.EndsWith("/")) resolvedApiBase += "/";
app.Logger.LogInformation("API BaseUrl: {api}", resolvedApiBase);

// -----------------------------------------------------------------------------
// 8) APPLY MIGRATIONS + SEED ROLES AND ADMIN
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

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Areas route for Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Simple health check for Docker/compose
app.MapGet("/healthz", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }));

app.Run();

// -----------------------------------------------------------------------------
// Identity Seeder (creates roles and admin on startup)
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
                await _users.AddToRoleAsync(admin, adminRole);
        }
        else
        {
            // Ensure role in case user existed from an older run
            if (!await _users.IsInRoleAsync(admin, adminRole))
                await _users.AddToRoleAsync(admin, adminRole);
        }
    }
}
