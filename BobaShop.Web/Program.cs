// ---------------------------------------------------------------
// Program.cs (BobaShop.Web) - Identity + Cookie auth
// .NET 9 minimal hosting style
// ---------------------------------------------------------------
using BobaShop.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB for Identity (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("IdentityDb")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
{
    // Relax a bit but still aligned to NIST-ish guidance
    opts.Password.RequireDigit = true;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireLowercase = true;
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

// MVC
builder.Services.AddControllersWithViews();

// Admin/Role seeding
builder.Services.AddScoped<IdentitySeeder>();

var app = builder.Build();

// Migrate / create Identity DB automatically (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    // Seed Admin + roles
    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await seeder.SeedAsync(
        app.Configuration["AdminSeed:Email"]!,
        app.Configuration["AdminSeed:Password"]!,
        app.Configuration["AdminSeed:Role"]!
    );
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // <-- IMPORTANT
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ---------------------------------------------------------------
// Local seeder service
// ---------------------------------------------------------------
public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole> _roles;
    private readonly UserManager<IdentityUser> _users;
    public IdentitySeeder(RoleManager<IdentityRole> roles, UserManager<IdentityUser> users)
    {
        _roles = roles; _users = users;
    }

    public async Task SeedAsync(string adminEmail, string adminPassword, string adminRole)
    {
        // Create roles
        var roles = new[] { "Admin", "Customer" };
        foreach (var r in roles)
            if (!await _roles.RoleExistsAsync(r))
                await _roles.CreateAsync(new IdentityRole(r));

        // Create admin user
        var admin = await _users.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await _users.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await _users.AddToRoleAsync(admin, adminRole);
        }
    }
}
