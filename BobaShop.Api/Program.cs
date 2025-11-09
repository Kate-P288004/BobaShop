// -----------------------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Entry point for the BoBatastic API.
//   Configures MongoDB, ASP.NET Identity on SQLite, JWT auth, API Versioning,
//   Swagger with Bearer, CORS, ProblemDetails, and Mongo seeding.
// -----------------------------------------------------------------------------

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using BobaShop.Api.Data;
using BobaShop.Api.Identity;     // ApplicationUser, AppIdentityDbContext
using BobaShop.Api.Models;
using BobaShop.Api.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// -----------------------------------------------------------------------------
// Helpers
// -----------------------------------------------------------------------------
static string BuildIdentityConnection(WebApplicationBuilder b)
{
    // Read from appsettings ConnectionStrings:IdentityConnection or default to local file
    var raw = b.Configuration.GetConnectionString("IdentityConnection") ?? "Data Source=identity.db";

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
// 1) MongoDB
// -----------------------------------------------------------------------------
builder.Services.Configure<MongoSettings>(config.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

// -----------------------------------------------------------------------------
// 2) ASP.NET Identity on SQLite
// -----------------------------------------------------------------------------
var identityCs = BuildIdentityConnection(builder);

builder.Services.AddDbContext<AppIdentityDbContext>(opts =>
    opts.UseSqlite(identityCs));

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
    .AddJsonOptions(_ =>
    {
        // Keep defaults for now to match Web client expectations
    });

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
var jwt = config.GetSection("Jwt");
var jwtKey = jwt["Key"];
var jwtIssuer = jwt["Issuer"];
var jwtAudience = jwt["Audience"];

// Dev fallback so startup does not explode if appsettings is missing.
// Change Key in appsettings for real use.
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = "DEV_ONLY_replace_me_with_long_random_secret_!234567890";
    builder.Logging.AddConsole(); // optional: ensures console logging
    builder.Services.AddLogging(); // optional: ensure logging services

    builder.Logging.ClearProviders();
    var tempLogger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger("BobaShop.Api");
    tempLogger.LogWarning("Jwt:Key not set. Using a development fallback key.");

}
if (string.IsNullOrWhiteSpace(jwtIssuer)) jwtIssuer = "BobaShop.Api";
if (string.IsNullOrWhiteSpace(jwtAudience)) jwtAudience = "BobaShop.Clients";

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// -----------------------------------------------------------------------------
// 6) Swagger / OpenAPI
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BoBatastic API v1",
        Version = "v1",
        Description = "Version 1 – Core CRUD endpoints for drinks, toppings, and orders."
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "BoBatastic API v2",
        Version = "v2",
        Description = "Version 2 – Demonstrates API versioning for AT2 evidence."
    });

    var bearer = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your_token}"
    };
    options.AddSecurityDefinition("Bearer", bearer);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { bearer, Array.Empty<string>() }
    });
});

// -----------------------------------------------------------------------------
// 7) CORS
// -----------------------------------------------------------------------------
const string CorsPolicyName = "BoBaCors";
string[] allowedOrigins =
    config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            // Do not call AllowCredentials with wildcard origins
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod(); // Dev fallback
        }
    });
});

// -----------------------------------------------------------------------------
// 8) ProblemDetails
// -----------------------------------------------------------------------------
builder.Services.AddProblemDetails();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 9) Log Identity connection and ensure migrations
// -----------------------------------------------------------------------------
app.Logger.LogInformation("IdentityConnection: {cs}", identityCs);

using (var scope = app.Services.CreateScope())
{
    var idDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

    var cs = idDb.Database.GetDbConnection().ConnectionString;
    var sb = new SqliteConnectionStringBuilder(cs);
    var dbFile = sb.DataSource ?? "identity.db";
    var absPath = Path.IsPathRooted(dbFile)
        ? dbFile
        : Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, dbFile));

    app.Logger.LogInformation("Identity DB file: {absPath}", absPath);

    await idDb.Database.MigrateAsync();

    // Seed roles and a demo admin if missing
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleMgr.RoleExistsAsync("Customer"))
        await roleMgr.CreateAsync(new IdentityRole("Customer"));
    if (!await roleMgr.RoleExistsAsync("Admin"))
        await roleMgr.CreateAsync(new IdentityRole("Admin"));

    const string adminEmail = "admin@bobatastic.local";
    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Demo Admin" };
        var created = await userMgr.CreateAsync(admin, "Admin123$");
        if (created.Succeeded)
            await userMgr.AddToRoleAsync(admin, "Admin");
    }
}

// -----------------------------------------------------------------------------
// 10) Middleware pipeline
// -----------------------------------------------------------------------------
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateProblemDetails(
            context,
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred.",
            detail: "Please try again or contact support if the issue persists."
        );

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "BoBatastic API v1");
        o.SwaggerEndpoint("/swagger/v2/swagger.json", "BoBatastic API v2");
    });
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// ============================================================================
// 11) Mongo seeding + index creation -----------------------------------------
// ============================================================================
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    try
    {
        // Run seeding and index setup
        DatabaseSeeder.Seed(ctx);
        IndexConfigurator.EnsureIndexes(ctx);

        // Resolve connection string from options (if present)
        var mongoOpts = scope.ServiceProvider.GetRequiredService<IOptions<MongoSettings>>().Value;
        var conn = mongoOpts?.ConnectionString ?? "(no ConnectionString)";

        // Get the drinks collection and derive DB name from it
        var col = ctx.Drinks; // if you don't have .Drinks, use: ctx.GetCollection<Drink>("Drinks")
        var dbName = col.Database.DatabaseNamespace.DatabaseName;

        // Count using empty filter (correct driver API)
        var before = await col.CountDocumentsAsync(Builders<Drink>.Filter.Empty);

        app.Logger.LogInformation("Mongo target => {conn} | DB={db} | Drinks(before)={before}",
            conn, dbName, before);

        // Idempotent seeding again (safe if your seeder uses upserts)
        DatabaseSeeder.Seed(ctx);

        var after = await col.CountDocumentsAsync(Builders<Drink>.Filter.Empty);
        app.Logger.LogInformation("Mongo seeded => Drinks(after)={after}", after);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database seeding or index creation failed.");
    }
}



// -----------------------------------------------------------------------------
// 12) Run
// -----------------------------------------------------------------------------
app.Run();

// -----------------------------------------------------------------------------
// End of File
// -----------------------------------------------------------------------------
