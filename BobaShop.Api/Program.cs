// -----------------------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Entry point for the BoBatastic API.
//   Configures MongoDB, ASP.NET Identity on SQLite, JWT auth, API Versioning,
//   Swagger with Bearer support, CORS, ProblemDetails, and Mongo seeding.
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
// Helper: build an absolute SQLite connection string for Identity
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
// -----------------------------------------------------------------------------
builder.Services.Configure<MongoSettings>(config.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.Configure<MediaSettings>(config.GetSection("Media"));


// -----------------------------------------------------------------------------
// 2) ASP.NET Identity on SQLite (with roles)
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
// -----------------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(_ => { });

// -----------------------------------------------------------------------------
// 4) API Versioning
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

            // Map standard role/name claims so [Authorize(Roles="Admin")] works
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

// -----------------------------------------------------------------------------
// 6) Authorization policies
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
            // Do NOT call .AllowCredentials() when using bearer tokens unless you truly need cookies
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
// -----------------------------------------------------------------------------
builder.Services.AddProblemDetails();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 10) Ensure Identity DB migrations + seed default roles + admin user
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

        // Optional: seed a service user for the Web app
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

    // Log DB file location
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
