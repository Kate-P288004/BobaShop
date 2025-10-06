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
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
