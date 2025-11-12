// -----------------------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Application entry point and composition root.
//   Registers services (MongoDB, Identity/SQLite, JWT auth, API Versioning,
//   Swagger, CORS, ProblemDetails), builds the middleware pipeline, runs seeders.
//   Notes: keep secrets out of source control; prefer environment variables for JWT.
// -----------------------------------------------------------------------------

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using BobaShop.Api.Data;
using BobaShop.Api.Identity;
using BobaShop.Api.Models;
using BobaShop.Api.Seed;
using BobaShop.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// -----------------------------------------------------------------------------
// Helper: BuildIdentityConnection
// Purpose:
//   Resolve a Data Source path to an absolute on-disk file for SQLite.
//   Supports both relative and absolute paths in appsettings.json.
// Why:
//   EF Core migrations and logging are clearer when the DB file is absolute.
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

// -----------------------------------------------------------------------------
// 1) MongoDB configuration and context
// Registers:
//   - IOptions<MongoSettings> from configuration section "Mongo"
//   - MongoDbContext as a singleton (safe because MongoClient is thread-safe)
// Also:
//   - Binds optional MediaSettings for preset images and static assets.
// -----------------------------------------------------------------------------
builder.Services.Configure<MongoSettings>(config.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.Configure<MediaSettings>(config.GetSection("Media"));

// -----------------------------------------------------------------------------
// 2) ASP.NET Identity on SQLite (with roles)
// Registers:
//   - AppIdentityDbContext (SQLite)
//   - Identity with ApplicationUser + roles + token providers
// Notes:
//   - Password policy kept simple but includes uppercase, digit, non-alphanumeric
//   - For production, consider stronger requirements and lockout settings
// -----------------------------------------------------------------------------
var identityCs = BuildIdentityConnection(builder);
builder.Services.AddDbContext<AppIdentityDbContext>(opts => opts.UseSqlite(identityCs));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 6;
        opt.Password.RequireDigit = true;
        opt.Password.RequireUppercase = true;
        opt.Password.RequireNonAlphanumeric = true;
    })
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

// -----------------------------------------------------------------------------
// 3) Controllers + JSON
// Notes:
//   - Customize JSON options here if you need specific casing or converters
// -----------------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(_ => { });

// -----------------------------------------------------------------------------
// 4) API Versioning
// Default:
//   v1.0 when unspecified. Versions appear in route as /api/v{version}/...
// Swashbuckle integration via AddApiExplorer for group naming.
// -----------------------------------------------------------------------------
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// -----------------------------------------------------------------------------
// 5) JWT Authentication
// Reads:
//   Jwt:Key, Jwt:Issuer, Jwt:Audience from configuration.
// Behavior:
//   - Falls back to a dev-only key when missing (logs a warning).
//   - Maps RoleClaimType and NameClaimType so [Authorize(Roles="Admin")] works.
// Security:
//   - Replace the fallback key in production.
//   - Prefer storing Jwt:Key in environment variables or a secret store.
// -----------------------------------------------------------------------------
var jwtSection = config.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
var jwtIssuer = string.IsNullOrWhiteSpace(jwtSection["Issuer"]) ? "BobaShop.Api" : jwtSection["Issuer"];
var jwtAudience = string.IsNullOrWhiteSpace(jwtSection["Audience"]) ? "BobaShop.Api" : jwtSection["Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = "DEV_ONLY_replace_me_with_long_random_secret_!234567890";
    var tempLogger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger("BobaShop.Api");
    tempLogger.LogWarning("Jwt:Key not set. Using a development fallback key.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true; // helpful during setup
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),

            // Role/Name mapping so [Authorize(Roles = "...")] matches "role" claims
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

// -----------------------------------------------------------------------------
// 6) Authorization policies
// Adds:
//   "RequireAdmin" policy that requires an authenticated user with the Admin role.
// Usage:
//   [Authorize(Policy = "RequireAdmin")] on admin-only endpoints.
// -----------------------------------------------------------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p =>
    {
        p.RequireAuthenticatedUser();
        p.RequireRole("Admin");
    });
});

// -----------------------------------------------------------------------------
// 7) Swagger / OpenAPI with Bearer
// Configures:
//   - Single document "v1" with title and description
//   - Bearer security definition and requirement
// Usage:
//   In Swagger UI, click "Authorize" and enter "Bearer {token}".
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BoBatastic API v1",
        Version = "v1",
        Description = "Core CRUD endpoints for drinks, toppings, and orders."
    });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { bearerScheme, Array.Empty<string>() }
    });
});

// -----------------------------------------------------------------------------
// 8) CORS
// Behavior:
//   - If "Cors:AllowedOrigins" is configured, restrict to that list.
//   - Else allow any origin (useful for local dev and Docker).
// Tip:
//   Do not call AllowCredentials with bearer tokens unless you need cookies.
// -----------------------------------------------------------------------------
const string CorsPolicyName = "BoBaCors";
var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// -----------------------------------------------------------------------------
// 9) ProblemDetails
// Adds RFC 7807 ProblemDetails responses for automatic error formatting.
// -----------------------------------------------------------------------------
builder.Services.AddProblemDetails();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 10) Identity migrations and seed (roles, admin, optional service user)
// Flow:
//   - Ensure database and migrations
//   - Ensure "Admin" role
//   - Create admin user if configured
//   - Add configured role to the admin
//   - Optionally create a service user for Web → API integration
// Logging:
//   - Writes the absolute path to the SQLite file for diagnostics
// -----------------------------------------------------------------------------
app.Logger.LogInformation("Identity DB connection: {cs}", identityCs);

using (var scope = app.Services.CreateScope())
{
    var idDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    await idDb.Database.MigrateAsync();

    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    if (!await roleMgr.RoleExistsAsync("Admin"))
        await roleMgr.CreateAsync(new IdentityRole("Admin"));

    var email = cfg["Admin:Email"];
    var password = cfg["Admin:Password"];
    var fullName = cfg["Admin:FullName"];
    var role = cfg["Admin:Role"] ?? "Admin";

    if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
    {
        var admin = await userMgr.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = email, Email = email, FullName = fullName, EmailConfirmed = true };
            var created = await userMgr.CreateAsync(admin, password);
            if (!created.Succeeded)
            {
                app.Logger.LogError("Failed to create admin user: {errs}",
                    string.Join(", ", created.Errors.Select(e => e.Description)));
            }
        }

        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));

        if (admin != null && !await userMgr.IsInRoleAsync(admin, role))
        {
            var added = await userMgr.AddToRoleAsync(admin, role);
            if (!added.Succeeded)
                app.Logger.LogError("Failed to add role '{role}' to admin user {email}: {errs}",
                    role, email, string.Join(", ", added.Errors.Select(e => e.Description)));
            else
                app.Logger.LogInformation("Ensured admin user {email} has role {role}.", email, role);
        }

        // Optional service user for background tasks or Web integration
        var svcEmail = cfg["Api:ServiceUser:Email"];
        var svcPassword = cfg["Api:ServiceUser:Password"];
        if (!string.IsNullOrWhiteSpace(svcEmail) && !string.IsNullOrWhiteSpace(svcPassword))
        {
            var svcUser = await userMgr.FindByEmailAsync(svcEmail);
            if (svcUser == null)
            {
                svcUser = new ApplicationUser { UserName = svcEmail, Email = svcEmail, FullName = "API Service User", EmailConfirmed = true };
                var createdSvc = await userMgr.CreateAsync(svcUser, svcPassword);
                if (!createdSvc.Succeeded)
                    app.Logger.LogError("Failed to create service user: {errs}", string.Join(", ", createdSvc.Errors.Select(e => e.Description)));
            }
            if (svcUser != null && !await userMgr.IsInRoleAsync(svcUser, "Admin"))
            {
                var addedSvc = await userMgr.AddToRoleAsync(svcUser, "Admin");
                if (!addedSvc.Succeeded)
                    app.Logger.LogError("Failed to add Admin to service user: {errs}", string.Join(", ", addedSvc.Errors.Select(e => e.Description)));
            }
        }
    }

    // Log absolute path to SQLite file to help diagnose local vs Docker paths
    var cs = idDb.Database.GetDbConnection().ConnectionString;
    var sb = new SqliteConnectionStringBuilder(cs);
    var dbFile = sb.DataSource ?? "identity.db";
    var absPath = Path.IsPathRooted(dbFile)
        ? dbFile
        : Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, dbFile));
    app.Logger.LogInformation("Identity DB file: {absPath}", absPath);
}

// -----------------------------------------------------------------------------
// 11) Middleware pipeline (order matters)
// Sequence:
//   - Swagger in Development
//   - HTTPS redirection, Static files
//   - CORS
//   - Authentication, Authorization
//   - MapControllers
// Notes:
//   If hosting behind a reverse proxy, configure ForwardedHeaders and HTTPS.
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "BoBatastic API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();      // serves /images/... from wwwroot over https
app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// -----------------------------------------------------------------------------
// 12) MongoDB seeding
// Flow:
//   - Run idempotent data seeders and ensure indexes
//   - Log target connection, DB name, and drink count for quick diagnostics
// Failure:
//   - Exceptions are caught and logged without crashing the host
// -----------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    try
    {
        DatabaseSeeder.Seed(ctx);
        IndexConfigurator.EnsureIndexes(ctx);

        var mongoOpts = scope.ServiceProvider.GetRequiredService<IOptions<MongoSettings>>().Value;
        var conn = mongoOpts?.ConnectionString ?? "(no ConnectionString)";
        var col = ctx.Drinks;
        var dbName = col.Database.DatabaseNamespace.DatabaseName;
        var count = await col.CountDocumentsAsync(Builders<Drink>.Filter.Empty);

        app.Logger.LogInformation("Mongo target => {conn} | DB={db} | Drinks(count)={count}",
            conn, dbName, count);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database seeding or index creation failed.");
    }
}

// -----------------------------------------------------------------------------
// 13) Run API
// -----------------------------------------------------------------------------
app.Run();

// -----------------------------------------------------------------------------
// End of File
// -----------------------------------------------------------------------------
