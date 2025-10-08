// -----------------------------------------------------------------------------
// File: ToppingsController.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: October 2025
// Description: CRUD + seed for toppings (soft delete + UTC audit fields).
// -----------------------------------------------------------------------------

using BobaShop.Api.Data;
using BobaShop.Api.Dtos;      // uses your ToppingCreateDto / ToppingUpdateDto
using BobaShop.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BobaShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ToppingsController : ControllerBase
{
    private readonly IMongoCollection<Topping> _toppings;

    public ToppingsController(MongoDbContext db) => _toppings = db.Toppings;

    // GET: api/v1/toppings?name=pearl&active=true&min=0.5&max=1.2
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Topping>>> GetAll(
        [FromQuery] string? name,
        [FromQuery] bool? active,
        [FromQuery] decimal? min,
        [FromQuery] decimal? max)
    {
        var filter = Builders<Topping>.Filter.Where(t => t.DeletedUtc == null);

        if (!string.IsNullOrWhiteSpace(name))
            filter &= Builders<Topping>.Filter.Regex(t => t.Name, new BsonRegularExpression(name, "i"));

        if (active.HasValue)
            filter &= Builders<Topping>.Filter.Eq(t => t.IsActive, active.Value);

        if (min.HasValue) filter &= Builders<Topping>.Filter.Gte(t => t.Price, min.Value);
        if (max.HasValue) filter &= Builders<Topping>.Filter.Lte(t => t.Price, max.Value);

        var list = await _toppings.Find(filter).SortBy(t => t.Name).ToListAsync();
        return Ok(list);
    }

    // GET: api/v1/toppings/{id}
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Topping>> GetById(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");
        var item = await _toppings.Find(t => t.Id == id && t.DeletedUtc == null).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    // POST: api/v1/toppings
    [HttpPost]
    public async Task<ActionResult<Topping>> Create([FromBody] ToppingCreateDto dto)
    {
        if (dto is null) return BadRequest("Body is required.");
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
        if (dto.Price < 0) return BadRequest("Price must be >= 0.");

        var entity = new Topping
        {
            Name = dto.Name.Trim(),
            Price = dto.Price,
            IsActive = dto.IsActive,           // map your DTO field name
            CreatedUtc = DateTime.UtcNow
        };

        await _toppings.InsertOneAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    // PUT: api/v1/toppings/{id}
    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, [FromBody] ToppingUpdateDto dto)
    {
        if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");
        if (dto is null) return BadRequest("Body is required.");
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
        if (dto.Price < 0) return BadRequest("Price must be >= 0.");

        var update = Builders<Topping>.Update
            .Set(t => t.Name, dto.Name.Trim())
            .Set(t => t.Price, dto.Price)
           .Set(t => t.IsActive, dto.IsActive)


            .Set(t => t.UpdatedUtc, DateTime.UtcNow);

        var result = await _toppings.UpdateOneAsync(
            t => t.Id == id && t.DeletedUtc == null, update);

        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    // PATCH: api/v1/toppings/{id}/active
    [HttpPatch("{id:length(24)}/active")]
    public async Task<IActionResult> SetActive(string id, [FromBody] bool isActive)
    {
        if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");

        var result = await _toppings.UpdateOneAsync(
            t => t.Id == id && t.DeletedUtc == null,
            Builders<Topping>.Update
                .Set(t => t.IsActive, isActive)
                .Set(t => t.UpdatedUtc, DateTime.UtcNow));

        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    // DELETE (soft): api/v1/toppings/{id}
    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> SoftDelete(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");

        var result = await _toppings.UpdateOneAsync(
            t => t.Id == id && t.DeletedUtc == null,
            Builders<Topping>.Update.Set(t => t.DeletedUtc, DateTime.UtcNow));

        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    // POST: api/v1/toppings/seed
    [HttpPost("seed")]
    public async Task<ActionResult> Seed()
    {
        var any = await _toppings.Find(t => true).Limit(1).AnyAsync();
        if (any) return Ok(new { message = "Toppings already exist." });

        var items = new[]
        {
            new Topping { Name = "Pearls",        Price = 0.80m, IsActive = true },
            new Topping { Name = "Grass Jelly",   Price = 0.70m, IsActive = true },
            new Topping { Name = "Pudding",       Price = 0.90m, IsActive = true },
            new Topping { Name = "Coconut Jelly", Price = 0.80m, IsActive = true }
        };

        await _toppings.InsertManyAsync(items);
        return Ok(new { inserted = items.Length });
    }
}
