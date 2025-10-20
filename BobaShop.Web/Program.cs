// ---------------------------------------------------------------
// File: Program.cs
// Student: Kate Odabas (P288004)
// Project: BoBaTastic – Web (.NET 9)
// Purpose: Identity + Cookie Auth + Session-backed Cart + Seeding
// ---------------------------------------------------------------
using BobaShop.Web.Data;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// 1) DATABASE (SQLite for Identity)
// ---------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("IdentityDb")));

// ---------------------------------------------------------------
// 2) IDENTITY (Cookie auth)
// ---------------------------------------------------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
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

// ---------------------------------------------------------------
// 3) MVC
// ---------------------------------------------------------------
builder.Services.AddControllersWithViews();

// ---------------------------------------------------------------
// 4) SESSION + CART SERVICE (for CartBadge VC)
// ---------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<ICartService, CartService>();

// ---------------------------------------------------------------
// 5) SEEDER
// ---------------------------------------------------------------
builder.Services.AddScoped<IdentitySeeder>();

var app = builder.Build();

// ---------------------------------------------------------------
// 6) APPLY MIGRATIONS + SEED ROLES/ADMIN
// ---------------------------------------------------------------
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

// ---------------------------------------------------------------
// 7) PIPELINE
// ---------------------------------------------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session MUST be before MVC endpoints and after routing
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ---------------------------------------------------------------
// Seeder class (kept here for convenience)
// ---------------------------------------------------------------
public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole> _roles;
    private readonly UserManager<IdentityUser> _users;

    public IdentitySeeder(RoleManager<IdentityRole> roles, UserManager<IdentityUser> users)
    {
        _roles = roles;
        _users = users;
    }

    public async Task SeedAsync(string adminEmail, string adminPassword, string adminRole)
    {
        // Roles
        var roles = new[] { "Admin", "Customer" };
        foreach (var r in roles)
        {
            if (!await _roles.RoleExistsAsync(r))
                await _roles.CreateAsync(new IdentityRole(r));
        }

        // Admin user
        var admin = await _users.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await _users.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await _users.AddToRoleAsync(admin, adminRole);
            }
        }
    }
}
