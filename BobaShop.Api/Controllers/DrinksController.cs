// -----------------------------------------------------------------------------
// File: DrinksController.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: October 2025
// Assessment: Diploma of IT – Application Development Project
// Description:
//   API v1 controller providing CRUD endpoints for managing drinks in MongoDB.
//   - Versioning (Asp.Versioning) => /api/v1/drinks
//   - Magic Three Dates: CreatedUtc, UpdatedUtc, DeletedUtc (soft delete)
//   - Read: [AllowAnonymous]
//   - Write: [Authorize(Roles = "Admin")]
// -----------------------------------------------------------------------------

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BobaShop.Api.Data;
using BobaShop.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using ApiVersionAttribute = Asp.Versioning.ApiVersionAttribute;

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
            _drinks = context.Drinks;
        }

        // Helpers
        private static bool IsValidObjectId(string id) => ObjectId.TryParse(id, out _);
        private static bool IsValidPercent(int? v) => v is null || v is 0 or 25 or 50 or 75 or 100;

        // -------------------------------------------------------------
        // GET: /api/v1/drinks?name=milk&active=true&min=5&max=9&sort=name&dir=asc&skip=0&take=20
        // Returns: 200 OK + list; X-Total-Count header for client paging.
        // -------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(List<Drink>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? name,
            [FromQuery] bool? active,
            [FromQuery] decimal? min,
            [FromQuery] decimal? max,
            [FromQuery] string? sort = "name",
            [FromQuery] string? dir = "asc",
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken ct = default)
        {
            if (take is < 1 or > 200) take = 50;
            if (skip < 0) skip = 0;

            var filter = Builders<Drink>.Filter.Eq(d => d.DeletedUtc, null);

            if (!string.IsNullOrWhiteSpace(name))
                filter &= Builders<Drink>.Filter.Regex(d => d.Name, new BsonRegularExpression(name, "i"));

            if (active.HasValue)
                filter &= Builders<Drink>.Filter.Eq(d => d.IsActive, active.Value);

            if (min.HasValue) filter &= Builders<Drink>.Filter.Gte(d => d.BasePrice, min.Value);
            if (max.HasValue) filter &= Builders<Drink>.Filter.Lte(d => d.BasePrice, max.Value);

            var countTask = _drinks.CountDocumentsAsync(filter, cancellationToken: ct);

            IFindFluent<Drink, Drink> query = _drinks.Find(filter);

            // Sort
            sort = sort?.Trim().ToLowerInvariant();
            dir = dir?.Trim().ToLowerInvariant();
            var ascending = dir is not "desc";

            query = sort switch
            {
                "price" => ascending ? query.SortBy(d => d.BasePrice) : query.SortByDescending(d => d.BasePrice),
                "created" => ascending ? query.SortBy(d => d.CreatedUtc) : query.SortByDescending(d => d.CreatedUtc),
                _ => ascending ? query.SortBy(d => d.Name) : query.SortByDescending(d => d.Name)
            };

            var itemsTask = query.Skip(skip).Limit(take).ToListAsync(ct);

            var total = await countTask;
            var items = await itemsTask;

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        // -------------------------------------------------------------
        // GET: /api/v1/drinks/{id}
        // -------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet("{id:length(24)}")]
        [ProducesResponseType(typeof(Drink), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id, CancellationToken ct = default)
        {
            if (!IsValidObjectId(id)) return BadRequest("Invalid id format.");

            var drink = await _drinks
                .Find(d => d.Id == id && d.DeletedUtc == null)
                .FirstOrDefaultAsync(ct);

            return drink is null ? NotFound() : Ok(drink);
        }

        // -------------------------------------------------------------
        // POST: /api/v1/drinks
        // Create a new drink.
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(Drink), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Drink model, CancellationToken ct = default)
        {
            if (model is null) return BadRequest("Body is required.");
            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required.");
            if (model.BasePrice < 0) return BadRequest("BasePrice must be >= 0.");
            if (!IsValidPercent(model.DefaultSugar)) return BadRequest("DefaultSugar must be 0,25,50,75,100.");
            if (!IsValidPercent(model.DefaultIce)) return BadRequest("DefaultIce must be 0,25,50,75,100.");

            model.Id = string.IsNullOrWhiteSpace(model.Id) ? ObjectId.GenerateNewId().ToString() : model.Id;
            model.Name = model.Name.Trim();
            model.Description = model.Description?.Trim() ?? string.Empty;
            model.CreatedUtc = DateTime.UtcNow;
            model.UpdatedUtc = null;
            model.DeletedUtc = null;

            try
            {
                await _drinks.InsertOneAsync(model, cancellationToken: ct);
                return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return Conflict("Duplicate key.");
            }
        }

        // -------------------------------------------------------------
        // PUT: /api/v1/drinks/{id}
        // Full update. Preserves CreatedUtc.
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:length(24)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] Drink model, CancellationToken ct = default)
        {
            if (!IsValidObjectId(id)) return BadRequest("Invalid id format.");
            if (model is null) return BadRequest("Body is required.");
            if (model.BasePrice < 0) return BadRequest("BasePrice must be >= 0.");
            if (!IsValidPercent(model.DefaultSugar)) return BadRequest("DefaultSugar must be 0,25,50,75,100.");
            if (!IsValidPercent(model.DefaultIce)) return BadRequest("DefaultIce must be 0,25,50,75,100.");

            var update = Builders<Drink>.Update
                .Set(d => d.Name, (model.Name ?? string.Empty).Trim())
                .Set(d => d.Description, model.Description?.Trim() ?? string.Empty)
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
                update,
                cancellationToken: ct);

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // -------------------------------------------------------------
        // PATCH: /api/v1/drinks/{id}/active
        // Toggle active flag only.
        // -------------------------------------------------------------
        public sealed class SetActiveDto { public bool IsActive { get; set; } }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:length(24)}/active")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetActive(string id, [FromBody] SetActiveDto body, CancellationToken ct = default)
        {
            if (!IsValidObjectId(id)) return BadRequest("Invalid id format.");
            if (body is null) return BadRequest("Body is required.");

            var result = await _drinks.UpdateOneAsync(
                d => d.Id == id && d.DeletedUtc == null,
                Builders<Drink>.Update
                    .Set(d => d.IsActive, body.IsActive)
                    .Set(d => d.UpdatedUtc, DateTime.UtcNow),
                cancellationToken: ct);

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // -------------------------------------------------------------
        // DELETE: /api/v1/drinks/{id}
        // Soft delete by setting DeletedUtc timestamp.
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:length(24)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(string id, CancellationToken ct = default)
        {
            if (!IsValidObjectId(id)) return BadRequest("Invalid id format.");

            var result = await _drinks.UpdateOneAsync(
                d => d.Id == id && d.DeletedUtc == null,
                Builders<Drink>.Update.Set(d => d.DeletedUtc, DateTime.UtcNow),
                cancellationToken: ct);

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // -------------------------------------------------------------
        // POST: /api/v1/drinks/seed
        // Seeds initial drinks (Debug: open; Release: Admin-only).
        // -------------------------------------------------------------
#if DEBUG
        [AllowAnonymous]
#else
        [Authorize(Roles = "Admin")]
#endif
        [HttpPost("seed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Seed(CancellationToken ct = default)
        {
            var existing = await _drinks
                .Find(d => d.DeletedUtc == null)
                .Limit(1)
                .AnyAsync(ct);

            if (existing) return Ok(new { message = "Drinks already exist." });

            var now = DateTime.UtcNow;
            var items = new[]
            {
                new Drink { Name = "Classic Milk Tea",  Description = "Black tea with milk",             BasePrice = 6.00m, DefaultSugar = 50, DefaultIce = 50, IsActive = true, CreatedUtc = now },
                new Drink { Name = "Brown Sugar Latte", Description = "Fresh milk + brown sugar syrup",  BasePrice = 7.50m, DefaultSugar = 75, DefaultIce = 50, IsActive = true, CreatedUtc = now },
                new Drink { Name = "Taro Smoothie",     Description = "Creamy taro blend",               BasePrice = 8.00m, DefaultSugar = 50, DefaultIce = 0,  IsActive = true, CreatedUtc = now },
                new Drink { Name = "Matcha Latte",      Description = "Japanese matcha + milk",          BasePrice = 7.00m, DefaultSugar = 50, DefaultIce = 50, IsActive = true, CreatedUtc = now }
            };

            await _drinks.InsertManyAsync(items, cancellationToken: ct);
            return Ok(new { inserted = items.Length });
        }
    }
}
