// -----------------------------------------------------------------------------
// File: DrinksController.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: October 2025
// Assessment: Diploma of IT – Application Development Project
// Description:
//   API v1 controller providing CRUD endpoints for managing drinks in MongoDB.
//   - Uses API Versioning (Asp.Versioning) => /api/v1/drinks
//   - Magic Three Dates: CreatedUtc, UpdatedUtc, DeletedUtc (soft delete)
//   - Read endpoints: [AllowAnonymous]
//   - Write endpoints: [Authorize(Roles = "Admin")]
// -----------------------------------------------------------------------------

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BobaShop.Api.Data;
using BobaShop.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Asp.Versioning.ApiVersion("1.0")]

    [Route("api/v{version:apiVersion}/[controller]")]
    public class DrinksController : ControllerBase
    {
        private readonly IMongoCollection<Drink> _drinks;

        public DrinksController(MongoDbContext context)
        {
            _drinks = context.Drinks; // or: context.GetCollection<Drink>("drinks");
        }

        // -------------------------------------------------------------
        // GET: /api/v1/drinks?name=milk&active=true&min=5&max=9
        // -------------------------------------------------------------
        [AllowAnonymous]
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

            if (min.HasValue) filter &= Builders<Drink>.Filter.Gte(d => d.BasePrice, min.Value);
            if (max.HasValue) filter &= Builders<Drink>.Filter.Lte(d => d.BasePrice, max.Value);

            var list = await _drinks.Find(filter).SortBy(d => d.Name).ToListAsync();
            return Ok(list);
        }

        // -------------------------------------------------------------
        // GET: /api/v1/drinks/{id}
        // Retrieve a specific drink by ObjectId string (24 chars).
        // -------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Drink>> GetById(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");
            var drink = await _drinks.Find(d => d.Id == id && d.DeletedUtc == null).FirstOrDefaultAsync();
            return drink is null ? NotFound() : Ok(drink);
        }

        // -------------------------------------------------------------
        // POST: /api/v1/drinks
        // Create a new drink (CreatedUtc set automatically).
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Drink>> Create([FromBody] Drink model)
        {
            if (model is null) return BadRequest("Body is required.");
            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required.");
            if (model.BasePrice < 0) return BadRequest("BasePrice must be >= 0.");

            model.Id = string.IsNullOrWhiteSpace(model.Id) ? ObjectId.GenerateNewId().ToString() : model.Id;
            model.CreatedUtc = DateTime.UtcNow;
            model.UpdatedUtc = null;
            model.DeletedUtc = null;

            await _drinks.InsertOneAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        // -------------------------------------------------------------
        // PUT: /api/v1/drinks/{id}
        // Full/partial update using $set; preserves CreatedUtc.
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] Drink model)
        {
            if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");
            if (model is null) return BadRequest("Body is required.");

            var update = Builders<Drink>.Update
                .Set(d => d.Name, (model.Name ?? string.Empty).Trim())
                .Set(d => d.Description, (model.Description ?? string.Empty).Trim())
                .Set(d => d.BasePrice, model.BasePrice)
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
        // PATCH: /api/v1/drinks/{id}/active
        // Toggle active flag only.
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin")]
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
        // DELETE: /api/v1/drinks/{id}
        // Soft delete by setting DeletedUtc timestamp.
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin")]
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
        // POST: /api/v1/drinks/seed
        // Quick seed for local testing. (Admin only)
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost("seed")]
        public async Task<ActionResult> Seed()
        {
            var any = await _drinks.Find(d => true).Limit(1).AnyAsync();
            if (any) return Ok(new { message = "Drinks already exist." });

            var items = new[]
            {
                new Drink { Name = "Classic Milk Tea", Description = "Black tea + milk", BasePrice = 6.00m, DefaultSugar = 50, DefaultIce = 50, IsActive = true, CreatedUtc = DateTime.UtcNow },
                new Drink { Name = "Brown Sugar Latte", Description = "Fresh milk + brown sugar syrup", BasePrice = 7.50m, DefaultSugar = 75, DefaultIce = 50, IsActive = true, CreatedUtc = DateTime.UtcNow },
                new Drink { Name = "Taro Smoothie", Description = "Creamy taro blend", BasePrice = 8.00m, DefaultSugar = 50, DefaultIce = 0,  IsActive = true, CreatedUtc = DateTime.UtcNow },
                new Drink { Name = "Matcha Latte", Description = "Japanese matcha + milk", BasePrice = 7.00m, DefaultSugar = 50, DefaultIce = 50, IsActive = true, CreatedUtc = DateTime.UtcNow }
            };

            await _drinks.InsertManyAsync(items);
            return Ok(new { inserted = items.Length });
        }
    }
}
