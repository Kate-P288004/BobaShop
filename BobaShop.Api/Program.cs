// ---------------------------------------------------------------
// File: Program.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: October 2025
// Purpose:
//   - Bind Mongo settings from appsettings.json
//   - Register MongoDbContext (singleton)
//   - Enable Controllers, Swagger, Versioning, CORS, and JWT Auth
//   - Provide global error handler (ProblemDetails)
//   - Seed the database + create indexes (idempotent)
// ---------------------------------------------------------------

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

// -----------------------------------------------------------------------------
// 1) MongoDB Configuration
// -----------------------------------------------------------------------------
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

// -----------------------------------------------------------------------------
// 2) MVC Controllers
// -----------------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        
    });

// -----------------------------------------------------------------------------
// 3) API Versioning (Asp.Versioning.Mvc)
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
// 4) JWT Authentication (Bearer Tokens)
// -----------------------------------------------------------------------------
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
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// -----------------------------------------------------------------------------
// 5) Swagger / OpenAPI (with grouped versions + Bearer support)
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BoBatastic API v1",
        Version = "v1",
        Description = "Version 1 – main REST API."
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "BoBatastic API v2",
        Version = "v2",
        Description = "Version 2 – used for versioning evidence."
    });

    // JWT security definition for Swagger "Authorize" button
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your_token}'"
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

// -----------------------------------------------------------------------------
// 6) CORS (Cross-Origin Resource Sharing)
// -----------------------------------------------------------------------------
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
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// -----------------------------------------------------------------------------
// 7) ProblemDetails (for consistent RFC7807 error responses)
// -----------------------------------------------------------------------------
builder.Services.AddProblemDetails();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 8) Middleware Pipeline
// -----------------------------------------------------------------------------

// Global error handler -> ProblemDetails JSON
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var problemDetailsFactory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = problemDetailsFactory.CreateProblemDetails(
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
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BoBatastic API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "BoBatastic API v2");
    });
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);

app.UseAuthentication();   // <-- now active
app.UseAuthorization();

app.MapControllers();

// -----------------------------------------------------------------------------
// 9) Database Seeding + Index Creation
// -----------------------------------------------------------------------------
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

// -----------------------------------------------------------------------------
// 10) Run the Application
// -----------------------------------------------------------------------------
app.Run();

// ---------------------------------------------------------------
// End of File
// ---------------------------------------------------------------
