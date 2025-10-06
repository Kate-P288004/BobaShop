using BobaShop.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// MongoDB Configuration (reads from appsettings.json)
// -----------------------------------------------------------------------------
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<MongoDbContext>();

// -----------------------------------------------------------------------------
// Add controllers, Swagger, and CORS
// -----------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) Policy
// -----------------------------------------------------------------------------
// CORS controls which web pages or client applications are allowed to access
// this API from a different domain, port, or protocol.
// For example, BobaShop.Web (frontend) runs on a different localhost port
// than BobaShop.Api (backend). Without CORS, browsers would block requests.
// The “AllowAll” policy is used here for development/testing so any origin
// can call this API. In production, restrict origins for security.
// -----------------------------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// -----------------------------------------------------------------------------
// Configure the HTTP request pipeline
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Apply the CORS policy so frontend applications can communicate with the API.
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
