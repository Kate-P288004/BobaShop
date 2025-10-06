using BobaShop.Api.Data;
using BobaShop.Api.Seed; // for DatabaseSeeder

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// MongoDB Configuration (reads from appsettings.json)
// -----------------------------------------------------------------------------
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

// -----------------------------------------------------------------------------
// Controllers, Swagger, and CORS
// -----------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) Policy
// -----------------------------------------------------------------------------
// Allows the frontend (running on a different localhost port) to call this API.
// “AllowAll” is fine for development; restrict origins in production.
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
// HTTP Request Pipeline
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll"); 
app.UseAuthorization();
app.MapControllers();

// -----------------------------------------------------------------------------
// Database Seeding
// -----------------------------------------------------------------------------
// On startup, seed default data if the collections are empty.
// This it only inserts when nothing exists.
// -----------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    DatabaseSeeder.Seed(ctx);
}

app.Run();
