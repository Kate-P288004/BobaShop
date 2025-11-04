// -----------------------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Entry point for the BobaShop API (BoBatastic).
//   This file configures services, middleware, and startup logic for the
//   ASP.NET Core Web API. Demonstrates advanced features including:
//     - MongoDB dependency injection
//     - Controller registration and JSON configuration
//     - API versioning (v1, v2) using Asp.Versioning
//     - JWT-based authentication and Swagger integration
//     - Cross-Origin Resource Sharing (CORS)
//     - ProblemDetails for unified error handling (RFC7807)
//     - Automatic database seeding and index creation
// -----------------------------------------------------------------------------

using BobaShop.Api.Data;
using BobaShop.Api.Seed;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 1) MongoDB Configuration
// Purpose:
//   Reads MongoDB settings (connection string, database name) from
//   appsettings.json, binds them to MongoSettings class, and registers
//   MongoDbContext as a singleton for dependency injection.
// ============================================================================
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

// ============================================================================
// 2) MVC Controllers Configuration
// Purpose:
//   Enables ASP.NET Core MVC-style controllers for routing HTTP requests
//   (GET, POST, PUT, DELETE) to endpoints. Adds JSON serialization options
//   for consistent camelCase property naming and indentation.
// ============================================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Example: options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // Example: options.JsonSerializerOptions.WriteIndented = true;
    });

// ============================================================================
// 3) API Versioning (Asp.Versioning.Mvc)
// Purpose:
//   Supports multiple API versions (v1, v2) simultaneously.
//   - Allows versioning via URL ( /api/v1/orders)
//   - Reports available API versions in HTTP headers
// ============================================================================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";            // e.g., v1, v2
    options.SubstituteApiVersionInUrl = true;      // auto-replaces {version} in route
});

// ============================================================================
// 4) JWT Authentication 
// Purpose:
//   Configures JSON Web Token authentication for secure API access.
//   - Reads key, issuer, and audience from appsettings.json
//   - Enforces signature validation, expiry, and claim integrity
// ============================================================================
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services
    .AddAuthentication("JwtBearer")
    .AddJwtBearer("JwtBearer", options =>
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
            ClockSkew = TimeSpan.FromMinutes(2) // allows small clock drift
        };
    });

// ============================================================================
// 5) Swagger / OpenAPI Configuration
// Purpose:
//   Automatically generates interactive API documentation.
//   - Defines endpoints for v1 and v2
//   - Integrates JWT Bearer authentication support for testing
// ============================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Register both versions for testing and versioning evidence
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

    // JWT Bearer configuration for Swagger Authorize button
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your_token}' below."
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

// ============================================================================
// 6) CORS (Cross-Origin Resource Sharing)
// Purpose:
//   Allows requests from the frontend web app (BobaShop.Web) to access the API.
//   Configurable through appsettings.json for security and flexibility.
// ============================================================================
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

const string CorsPolicyName = "BoBaCors";

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
            // Development fallback: allow all origins
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// ============================================================================
// 7) ProblemDetails (RFC7807 Standardized Error Responses)
// Purpose:
//   Provides consistent, machine-readable error responses across the API.
//   Simplifies debugging and aligns with RESTful best practices.
// ============================================================================
builder.Services.AddProblemDetails();

var app = builder.Build();

// ============================================================================
// 8) Middleware Pipeline Configuration
// Purpose:
//   Defines the sequence of middleware executed for each HTTP request.
//   Includes error handling, Swagger, authentication, authorization, and routing.
// ============================================================================

// Global exception handler -> returns ProblemDetails JSON
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

// Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BoBatastic API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "BoBatastic API v2");
    });
}

// Enforce HTTPS, CORS, and authentication/authorization
app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes (/api/v1/drinks)
app.MapControllers();

// ============================================================================
// 9) Database Seeding + Index Creation
// Purpose:
//   Initializes MongoDB with default data (Drinks, etc.) and ensures that
//   required indexes are created for performance and query optimization.
// ============================================================================
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    try
    {
        DatabaseSeeder.Seed(ctx);                 // Insert default data
        IndexConfigurator.EnsureIndexes(ctx);     // Create unique indexes 
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database seeding or index creation failed.");
    }
}

// ============================================================================
// 10) Run the Application
// Purpose:
//   Starts the web server and begins listening for incoming HTTP requests.
// ============================================================================
app.Run();

// -----------------------------------------------------------------------------
// End of File
// -----------------------------------------------------------------------------
