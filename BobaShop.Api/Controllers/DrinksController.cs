// -----------------------------------------------------------------------------
// File: DrinksController.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: October 2025
// Assessment: Diploma of IT – Application Development Project
// Description:
// API controller providing CRUD endpoints for managing drinks in MongoDB.
// Demonstrates use of the Magic Three Dates for record lifecycle tracking.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using BobaShop.Api.Data;
using BobaShop.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DrinksController : ControllerBase
    {
        private readonly IMongoCollection<Drink> _drinks;

        public DrinksController(MongoDbContext context)
        {
            // If your context exposes a property, use: context.Drinks;
            // If it exposes a GetCollection<T>(name) method, use the line below:
            _drinks = context.Drinks; // or: context.GetCollection<Drink>("drinks");
        }

        // -------------------------------------------------------------
        // GET: api/v1/drinks?name=milk&active=true&min=5&max=9
        // Returns drinks that are not soft-deleted, with optional filters.
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<List<Drink>>> GetAll(
            [FromQuery] string? name,
            [FromQuery] bool? active,
            [FromQuery] decimal? min,
            [FromQuery] decimal? max)
        {
            var filter = Builders<Drink>.Filter.Where(d => d.DeletedUtc == null);

            if (!string.IsNullOrWhiteSpace(name))
                filter &= Builders<Drink>.Filter.Regex(d => d.Name, new BsonRegularExpression(name, "i"));

            if (active.HasValue)
                filter &= Builders<Drink>.Filter.Eq(d => d.IsActive, active.Value);

            if (min.HasValue) filter &= Builders<Drink>.Filter.Gte(d => d.Price, min.Value);
            if (max.HasValue) filter &= Builders<Drink>.Filter.Lte(d => d.Price, max.Value);

            var list = await _drinks.Find(filter).SortBy(d => d.Name).ToListAsync();
            return Ok(list);
        }

        // -------------------------------------------------------------
        // GET: api/v1/drinks/{id}
        // Retrieve a specific drink by ObjectId string (24 chars).
        // -------------------------------------------------------------
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Drink>> GetById(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");
            var drink = await _drinks.Find(d => d.Id == id && d.DeletedUtc == null).FirstOrDefaultAsync();
            return drink is null ? NotFound() : Ok(drink);
        }

        // -------------------------------------------------------------
        // POST: api/v1/drinks
        // Create a new drink (CreatedUtc set automatically).
        // -------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Drink>> Create([FromBody] Drink model)
        {
            if (model is null) return BadRequest("Body is required.");
            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required.");
            if (model.BasePrice < 0 || model.Price < 0) return BadRequest("Prices must be >= 0.");

            model.Id = string.IsNullOrWhiteSpace(model.Id) ? ObjectId.GenerateNewId().ToString() : model.Id;
            model.CreatedUtc = DateTime.UtcNow;
            model.UpdatedUtc = null;
            model.DeletedUtc = null;

            await _drinks.InsertOneAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        // -------------------------------------------------------------
        // PUT: api/v1/drinks/{id}
        // Partial update using $set; preserves CreatedUtc.
        // -------------------------------------------------------------
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] Drink model)
        {
            if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");
            if (model is null) return BadRequest("Body is required.");

            var update = Builders<Drink>.Update
                .Set(d => d.Name, (model.Name ?? string.Empty).Trim())
                .Set(d => d.Description, (model.Description ?? string.Empty).Trim())
                .Set(d => d.BasePrice, model.BasePrice)
                .Set(d => d.Price, model.Price)
                .Set(d => d.SmallUpcharge, model.SmallUpcharge)
                .Set(d => d.MediumUpcharge, model.MediumUpcharge)
                .Set(d => d.LargeUpcharge, model.LargeUpcharge)
                .Set(d => d.DefaultSugar, model.DefaultSugar)
                .Set(d => d.DefaultIce, model.DefaultIce)
                .Set(d => d.IsActive, model.IsActive)
                .Set(d => d.UpdatedUtc, DateTime.UtcNow);

            var result = await _drinks.UpdateOneAsync(
                d => d.Id == id && d.DeletedUtc == null,
                update);

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // -------------------------------------------------------------
        // PATCH: api/v1/drinks/{id}/active
        // Toggle active flag only.
        // -------------------------------------------------------------
        [HttpPatch("{id:length(24)}/active")]
        public async Task<IActionResult> SetActive(string id, [FromBody] bool isActive)
        {
            if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");

            var result = await _drinks.UpdateOneAsync(
                d => d.Id == id && d.DeletedUtc == null,
                Builders<Drink>.Update
                    .Set(d => d.IsActive, isActive)
                    .Set(d => d.UpdatedUtc, DateTime.UtcNow));

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // -------------------------------------------------------------
        // DELETE: api/v1/drinks/{id}
        // Soft delete by setting DeletedUtc timestamp.
        // -------------------------------------------------------------
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");

            var result = await _drinks.UpdateOneAsync(
                d => d.Id == id && d.DeletedUtc == null,
                Builders<Drink>.Update.Set(d => d.DeletedUtc, DateTime.UtcNow));

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // -------------------------------------------------------------
        // POST: api/v1/drinks/seed
        // Quick seed for local testing.
        // -------------------------------------------------------------
        [HttpPost("seed")]
        public async Task<ActionResult> Seed()
        {
            var any = await _drinks.Find(d => true).Limit(1).AnyAsync();
            if (any) return Ok(new { message = "Drinks already exist." });

            var items = new[]
            {
                new Drink { Name = "Classic Milk Tea", Description = "Black tea + milk", BasePrice = 6.00m, Price = 6.00m, DefaultSugar = 50, DefaultIce = 50 },
                new Drink { Name = "Brown Sugar Latte", Description = "Fresh milk + brown sugar syrup", BasePrice = 7.00m, Price = 7.50m, DefaultSugar = 75, DefaultIce = 50 },
                new Drink { Name = "Taro Smoothie", Description = "Creamy taro blend", BasePrice = 7.00m, Price = 8.00m, DefaultSugar = 50, DefaultIce = 0 },
                new Drink { Name = "Matcha Latte", Description = "Japanese matcha + milk", BasePrice = 6.50m, Price = 7.00m, DefaultSugar = 50, DefaultIce = 50 }
            };

            await _drinks.InsertManyAsync(items);
            return Ok(new { inserted = items.Length });
        }
    }
}
