// -----------------------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Web (BoBaTastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// -----------------------------------------------------------------------------
// Description:
//   Entry point for the BoBatastic Web frontend (MVC application).
//   Sets up Identity (SQLite), session, DI for cart/rewards, API HttpClient,
//   authorization policy, and startup migrations with admin seeding.
// -----------------------------------------------------------------------------

using System.Net.Http.Headers;
using BobaShop.Web.Data;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 1) DATABASE (SQLite for Identity) — build absolute path to identity.db
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

// Persist Data Protection keys between restarts (stable cookies)
var keysPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "BobaShop", "keys");
Directory.CreateDirectory(keysPath);

builder.Services
    .AddDataProtection()
    .SetApplicationName("BobaShop.Web")
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

// -----------------------------------------------------------------------------
// 2) IDENTITY (users + roles) — password rules, cookies, tokens
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
// 3) MVC + Razor
// -----------------------------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(o => o.LowercaseUrls = true);

// -----------------------------------------------------------------------------
// 4) SESSION + CART — require distributed cache for Session to work
// -----------------------------------------------------------------------------
builder.Services.AddDistributedMemoryCache(); // important for Session
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddScoped<ICartService, CartService>();

// -----------------------------------------------------------------------------
// 5) REWARDS
// -----------------------------------------------------------------------------
builder.Services.AddScoped<IRewardsService, RewardsService>();

// -----------------------------------------------------------------------------
// 6) API INTEGRATION — HttpClient, JWT helper (if service uses tokens)
// -----------------------------------------------------------------------------
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IApiAuthService, ApiAuthService>();

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
// 7) ADMIN SEEDER + POLICY
// -----------------------------------------------------------------------------
builder.Services.AddScoped<IdentitySeeder>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
});

// -----------------------------------------------------------------------------
// 8) BUILD + LOG
// -----------------------------------------------------------------------------
var app = builder.Build();

app.Logger.LogInformation("Identity DB: {cs}", identityCs);

var resolvedApiBase = app.Configuration["Api:BaseUrl"] ?? "https://localhost:7274/";
if (!resolvedApiBase.EndsWith("/")) resolvedApiBase += "/";
app.Logger.LogInformation("API BaseUrl: {api}", resolvedApiBase);

// -----------------------------------------------------------------------------
// 9) MIGRATIONS + ADMIN SEED
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
// 10) PIPELINE
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

// Areas (e.g., /Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Simple health endpoint
app.MapGet("/healthz", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }));

app.Run();

// -----------------------------------------------------------------------------
// IdentitySeeder — ensures roles and an admin user exist
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
            if (!await _roles.RoleExistsAsync(r))
                await _roles.CreateAsync(new IdentityRole(r));

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
        else if (!await _users.IsInRoleAsync(admin, adminRole))
        {
            await _users.AddToRoleAsync(admin, adminRole);
        }
    }
}
