// -----------------------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Entry point for the BoBatastic API.
//   Configures MongoDB, ASP.NET Identity (SQLite), JWT auth, API Versioning,
//   Swagger (with Bearer), CORS, ProblemDetails, and Mongo seeding.
// -----------------------------------------------------------------------------

using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using BobaShop.Api.Data;
using BobaShop.Api.Identity;     // ApplicationUser, AppIdentityDbContext
using BobaShop.Api.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 0) Configuration helpers
// ============================================================================
var config = builder.Configuration;

// ============================================================================
// 1) MongoDB (Domain data) ----------------------------------------------------
// Reads "Mongo" section; registers MongoDbContext (singleton).
// ============================================================================
builder.Services.Configure<MongoSettings>(config.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

// ============================================================================
// 2) ASP.NET Identity (Users/Roles) + SQLite ---------------------------------
// Uses a local SQLite db for Identity (same db file as Web if desired).
// appsettings.json:
// "ConnectionStrings": { "IdentityConnection": "Data Source=identity.db" }
// ============================================================================
builder.Services.AddDbContext<AppIdentityDbContext>(opts =>
    opts.UseSqlite(config.GetConnectionString("IdentityConnection")));

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

// ============================================================================
// 3) Controllers + JSON options ----------------------------------------------
// ============================================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // options.JsonSerializerOptions.WriteIndented = true;
    });

// ============================================================================
// 4) API Versioning (Asp.Versioning) -----------------------------------------
// URL versioning like /api/v1/drinks and reports available versions in headers.
// ============================================================================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";       // v1, v2
    options.SubstituteApiVersionInUrl = true;
});

// ============================================================================
// 5) JWT Authentication -------------------------------------------------------
// Reads "Jwt": { Key, Issuer, Audience } from appsettings.json.
// ============================================================================
var jwt = config.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

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
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// ============================================================================
// 6) Swagger / OpenAPI --------------------------------------------------------
// Adds two docs (v1, v2) and configures the Bearer auth button.
// ============================================================================
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

// ============================================================================
// 7) CORS ---------------------------------------------------------------------
// Allows calls from the MVC frontend (BobaShop.Web). In Dev, allow all.
// appsettings.json:
// "Cors": { "AllowedOrigins": ["https://localhost:7243","http://localhost:5262"] }
// ============================================================================
const string CorsPolicyName = "BoBaCors";
string[] allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        else
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // Dev fallback
    });
});

// ============================================================================
// 8) ProblemDetails (RFC7807) -------------------------------------------------
// ============================================================================
builder.Services.AddProblemDetails();

var app = builder.Build();

// ============================================================================
// 9) Identity DB: log path + ensure migrations -------------------------------
// This guarantees the API points to the expected SQLite file and that the
// Identity schema exists (AspNetUsers, etc.).
// ============================================================================
app.Logger.LogInformation("IdentityConnection: {cs}", builder.Configuration.GetConnectionString("IdentityConnection"));

using (var scope = app.Services.CreateScope())
{
    // --- Ensure Identity DB is migrated and log absolute path
    var idDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

    var cs = idDb.Database.GetDbConnection().ConnectionString;
    var sb = new SqliteConnectionStringBuilder(cs);
    var absPath = Path.GetFullPath(sb.DataSource ?? "identity.db");
    app.Logger.LogInformation("Identity DB file: {absPath}", absPath);

    idDb.Database.Migrate();

    // (Optional) seed roles/admin once if you need role-protected endpoints
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleMgr.RoleExistsAsync("Customer"))
        await roleMgr.CreateAsync(new IdentityRole("Customer"));

    if (!await roleMgr.RoleExistsAsync("Admin"))
        await roleMgr.CreateAsync(new IdentityRole("Admin"));

    // Demo admin (optional)
    const string adminEmail = "admin@bobatastic.local";
    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Demo Admin" };
        await userMgr.CreateAsync(admin, "Admin123$");
        await userMgr.AddToRoleAsync(admin, "Admin");
    }
}

// ============================================================================
// 10) Middleware pipeline -----------------------------------------------------
// ============================================================================
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
        DatabaseSeeder.Seed(ctx);
        IndexConfigurator.EnsureIndexes(ctx);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database seeding or index creation failed.");
    }
}

// ============================================================================
// 12) Run ---------------------------------------------------------------------
// ============================================================================
app.Run();

// -----------------------------------------------------------------------------
// End of File
// -----------------------------------------------------------------------------
